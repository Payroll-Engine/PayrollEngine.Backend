using System;
using System.Threading;
using Task = System.Threading.Tasks.Task;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Application;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Application.Service;

namespace PayrollEngine.Api.Core;

/// <summary>
/// Background service that dequeues and processes payrun jobs.
/// <para>
/// Job lifecycle ownership:
/// <list type="bullet">
///   <item><b>Abort</b> – set and persisted by <see cref="PayrunProcessor"/> (via AbortJobAsync),
///         which also sets <see cref="PayrunJob.JobEnd"/>.</item>
///   <item><b>Complete</b> – set and persisted by this worker after the processor returns
///         without setting <see cref="PayrunJob.JobEnd"/> (success path).</item>
///   <item><b>Infrastructure abort</b> – set by this worker when the processor throws
///         an unhandled exception or the service shuts down.</item>
/// </list>
/// The worker uses <see cref="PayrunJob.JobEnd"/> (not <see cref="PayrunJob.JobStatus"/>)
/// to detect whether the processor already finalized the job, because the invocation's
/// <c>JobStatus</c> (the desired target status) is applied to the job object before
/// processing starts and does not indicate actual completion.
/// </para>
/// </summary>
public class PayrunJobWorkerService : BackgroundService
{
    private IPayrunJobQueue Queue { get; }
    private IServiceScopeFactory ScopeFactory { get; }

    /// <summary>
    /// Initializes a new instance of the PayrunJobWorkerService
    /// </summary>
    public PayrunJobWorkerService(
        IPayrunJobQueue queue,
        IServiceScopeFactory scopeFactory)
    {
        Queue = queue ?? throw new ArgumentNullException(nameof(queue));
        ScopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.Debug("Payrun Job Worker Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            PayrunJobQueueItem queueItem = null;
            try
            {
                // Wait for a job to be available
                queueItem = await Queue.DequeueAsync(stoppingToken);

                Log.Debug(
                    $"Processing payrun job {queueItem.PayrunJobId} for tenant {queueItem.TenantId}");

                await ProcessJobAsync(queueItem);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Graceful shutdown - mark in-progress job as aborted if exists
                if (queueItem != null)
                {
                    await MarkJobAbortedAsync(queueItem, "Payrun worker service shutdown.");
                }
                break;
            }
            catch (Exception ex)
            {
                Log.Error(ex,
                    $"Error processing payrun job {queueItem?.PayrunJobId}: {ex.GetBaseMessage()}.");

                if (queueItem != null)
                {
                    await MarkJobAbortedAsync(queueItem, ex.GetBaseMessage());
                }
            }
        }

        Log.Debug("Payrun Job Worker Service stopped");
    }

    /// <summary>
    /// Processes a single payrun job from the queue.
    /// <para>
    /// The processor sets <see cref="PayrunJob.JobEnd"/> only when it finalizes the job
    /// itself (abort scenarios via <c>AbortJobAsync</c>). On the success path, the
    /// processor returns the job with <c>JobEnd == null</c>, signalling that the worker
    /// is responsible for finalization.
    /// </para>
    /// <para>
    /// Note: <see cref="PayrunJob.JobStatus"/> cannot be used to detect finalization
    /// because the invocation's target status (e.g. <c>Complete</c>) is applied to
    /// the job object in <c>CreateOrLoadJobAsync</c> before processing starts.
    /// </para>
    /// </summary>
    private async Task ProcessJobAsync(
        PayrunJobQueueItem queueItem)
    {
        // Create a new scope for each job to get fresh scoped services
        using var scope = ScopeFactory.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        // Get required services
        var dbContext = serviceProvider.GetRequiredService<IDbContext>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var serverConfiguration = configuration.GetConfiguration<PayrollServerConfiguration>();
        var scriptProvider = serviceProvider.GetRequiredService<IScriptProvider>();
        var webhookDispatcher = serviceProvider.GetRequiredService<IWebhookDispatchService>();

        // Build processor settings from scoped services
        var processorSettings = BuildProcessorSettings(serviceProvider, serverConfiguration, scriptProvider);

        // Create and run processor
        var processor = new PayrunProcessor(
            queueItem.Tenant,
            queueItem.Payrun,
            processorSettings);

        // Process the job (this is the long-running operation)
        var payrunJob = await processor.Process(queueItem.JobInvocation);

        // Finalize based on the actual processor result.
        // The processor sets JobEnd only when it finalizes the job itself (AbortJobAsync).
        // On the success path, JobEnd remains null → worker must finalize.
        if (payrunJob.JobEnd != null)
        {
            // Processor already finalized (abort or retro-complete) and persisted
            Log.Debug(
                $"Payrun job {payrunJob.Id} finished with processor status {payrunJob.JobStatus}.");
        }
        else
        {
            // Success path: processor returned without finalizing
            await FinalizeJobCompletedAsync(queueItem, payrunJob);
            Log.Debug(
                $"Finalized payrun job {payrunJob.Id} as {PayrunJobStatus.Complete}.");
        }

        // Send webhook notification with the actual job status
        await SendJobCompletionWebhookAsync(
            dbContext,
            webhookDispatcher,
            queueItem.TenantId,
            payrunJob,
            payrunJob.CreatedUserId);
    }

    private static PayrunProcessorSettings BuildProcessorSettings(
        IServiceProvider serviceProvider,
        PayrollServerConfiguration serverConfiguration,
        IScriptProvider scriptProvider)
    {
        return new PayrunProcessorSettings
        {
            DbContext = serviceProvider.GetRequiredService<IDbContext>(),
            UserRepository = serviceProvider.GetRequiredService<IUserRepository>(),
            DivisionRepository = serviceProvider.GetRequiredService<IDivisionRepository>(),
            TaskRepository = serviceProvider.GetRequiredService<ITaskRepository>(),
            LogRepository = serviceProvider.GetRequiredService<ILogRepository>(),
            EmployeeRepository = serviceProvider.GetRequiredService<IEmployeeRepository>(),
            GlobalCaseValueRepository = serviceProvider.GetRequiredService<IGlobalCaseValueRepository>(),
            NationalCaseValueRepository = serviceProvider.GetRequiredService<INationalCaseValueRepository>(),
            CompanyCaseValueRepository = serviceProvider.GetRequiredService<ICompanyCaseValueRepository>(),
            EmployeeCaseValueRepository = serviceProvider.GetRequiredService<IEmployeeCaseValueRepository>(),
            PayrunJobRepository = serviceProvider.GetRequiredService<IPayrunJobRepository>(),
            RegulationLookupSetRepository = serviceProvider.GetRequiredService<ILookupSetRepository>(),
            RegulationRepository = serviceProvider.GetRequiredService<IRegulationRepository>(),
            RegulationShareRepository = serviceProvider.GetRequiredService<IRegulationShareRepository>(),
            PayrollRepository = serviceProvider.GetRequiredService<IPayrollRepository>(),
            PayrollResultRepository = serviceProvider.GetRequiredService<IPayrollResultRepository>(),
            PayrollConsolidatedResultRepository = serviceProvider.GetRequiredService<IPayrollConsolidatedResultRepository>(),
            PayrollResultSetRepository = serviceProvider.GetRequiredService<IPayrollResultSetRepository>(),
            CalendarRepository = serviceProvider.GetRequiredService<ICalendarRepository>(),
            PayrollCalculatorProvider = serviceProvider.GetRequiredService<IPayrollCalculatorProvider>(),
            WebhookDispatchService = serviceProvider.GetRequiredService<IWebhookDispatchService>(),
            FunctionLogTimeout = serverConfiguration.FunctionLogTimeout,
            LogEmployeeTiming = serverConfiguration.LogEmployeeTiming,
            AssemblyCacheTimeout = serverConfiguration.AssemblyCacheTimeout,
            MaxParallelEmployees = serverConfiguration.GetMaxParallelEmployees(),
            MaxRetroPayrunPeriods = serverConfiguration.MaxRetroPayrunPeriods,
            ScriptProvider = scriptProvider
        };
    }

    /// <summary>
    /// Marks a payrun job as aborted due to an infrastructure failure
    /// (unhandled exception, service shutdown).
    /// <para>
    /// Only updates the job if <see cref="PayrunJob.JobEnd"/> is not already set,
    /// preventing overwrites of processor-set abort states.
    /// </para>
    /// </summary>
    /// <param name="queueItem">The queue item identifying the job.</param>
    /// <param name="reason">Human-readable abort reason.</param>
    private async Task MarkJobAbortedAsync(PayrunJobQueueItem queueItem, string reason)
    {
        try
        {
            using var scope = ScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<IDbContext>();
            var payrunJobRepository = scope.ServiceProvider.GetRequiredService<IPayrunJobRepository>();

            var job = await payrunJobRepository.GetAsync(
                dbContext, queueItem.TenantId, queueItem.PayrunJobId);

            if (job != null && job.JobEnd == null)
            {
                job.JobStatus = PayrunJobStatus.Abort;
                job.JobEnd = Date.Now;
                job.ErrorMessage = reason;
                job.Message = $"Job aborted: {reason}";

                await payrunJobRepository.UpdateAsync(dbContext, queueItem.TenantId, job);

                Log.Warning(
                    $"Marked payrun job {queueItem.PayrunJobId} as aborted: {reason}.");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex,
                $"Failed to mark job {queueItem.PayrunJobId} as aborted.");
        }
    }

    /// <summary>
    /// Finalizes a payrun job as completed after successful processor execution.
    /// Only called when the processor returned without setting <see cref="PayrunJob.JobEnd"/>.
    /// Uses <see cref="PayrunJobStatus.Complete"/> as the target status, independent
    /// of the original invocation request status.
    /// </summary>
    /// <param name="queueItem">The queue item identifying the job.</param>
    /// <param name="payrunJob">The in-memory job object returned by the processor,
    /// updated with the final status for consistent webhook dispatch.</param>
    private async Task FinalizeJobCompletedAsync(PayrunJobQueueItem queueItem, PayrunJob payrunJob)
    {
        try
        {
            using var scope = ScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<IDbContext>();
            var payrunJobRepository = scope.ServiceProvider.GetRequiredService<IPayrunJobRepository>();

            var job = await payrunJobRepository.GetAsync(
                dbContext, queueItem.TenantId, queueItem.PayrunJobId);

            if (job != null && job.JobEnd == null)
            {
                job.JobStatus = PayrunJobStatus.Complete;
                job.JobEnd = Date.Now;

                var duration = job.JobEnd.Value - job.JobStart;
                var durationText = duration < TimeSpan.FromSeconds(1)
                    ? $"{duration.TotalMilliseconds:F2} ms"
                    : duration.ToReadableString();
                job.Message = $"Completed payrun calculation successfully in {durationText}.";

                await payrunJobRepository.UpdateAsync(dbContext, queueItem.TenantId, job);

                // Update the in-memory object so webhook dispatch uses consistent data
                payrunJob.JobStatus = job.JobStatus;
                payrunJob.JobEnd = job.JobEnd;
                payrunJob.Message = job.Message;

                Log.Debug(
                    $"Marked payrun job {queueItem.PayrunJobId} as completed in {durationText}.");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex,
                $"Failed to mark job {queueItem.PayrunJobId} as completed.");
        }
    }

    /// <summary>
    /// Sends a webhook notification for job completion or abort.
    /// Uses the actual job status from the in-memory object, which is kept
    /// in sync with the database by <see cref="FinalizeJobCompletedAsync"/>.
    /// Webhook failures are logged but do not fail the job.
    /// </summary>
    private static async Task SendJobCompletionWebhookAsync(
        IDbContext dbContext,
        IWebhookDispatchService webhookDispatcher,
        int tenantId,
        PayrunJob payrunJob,
        int userId)
    {
        try
        {
            // No webhooks configured, skip dispatch
            if (!await webhookDispatcher.HasWebhooksAsync(dbContext, tenantId))
            {
                return;
            }

            var action = payrunJob.JobStatus == PayrunJobStatus.Complete
                ? WebhookAction.PayrunJobFinish
                : WebhookAction.PayrunJobProcess;

            var json = Serialization.DefaultJsonSerializer.Serialize(payrunJob);

            await webhookDispatcher.SendMessageAsync(dbContext, tenantId,
                new()
                {
                    Action = action,
                    RequestMessage = json
                },
                userId: userId);
        }
        catch (Exception ex)
        {
            // Webhook failure should not fail the job
            Log.Warning(ex, $"Failed to send webhook for job {payrunJob.Id}.");
        }
    }
}
