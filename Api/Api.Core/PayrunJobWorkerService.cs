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
/// Background service that processes payrun jobs from the queue
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

        await MarkJobCompletedAsync(queueItem, "Payrun worker service completed.");
        Log.Debug(
            $"Completed payrun job {payrunJob.Id} with status {payrunJob.JobStatus}.");

        // Send webhook notification for job completion
        await SendJobCompletionWebhookAsync(
            dbContext,
            webhookDispatcher,
            queueItem.TenantId,
            payrunJob,
            queueItem.JobInvocation.UserId);
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
            PayrunRepository = serviceProvider.GetRequiredService<IPayrunRepository>(),
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
            AssemblyCacheTimeout = serverConfiguration.AssemblyCacheTimeout,
            ScriptProvider = scriptProvider
        };
    }

    private async Task MarkJobAbortedAsync(PayrunJobQueueItem queueItem, string reason)
    {
        try
        {
            using var scope = ScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<IDbContext>();
            var payrunJobRepository = scope.ServiceProvider.GetRequiredService<IPayrunJobRepository>();

            var job = await payrunJobRepository.GetAsync(
                dbContext, queueItem.TenantId, queueItem.PayrunJobId);

            if (job != null && !job.JobStatus.IsFinal())
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

    private async Task MarkJobCompletedAsync(PayrunJobQueueItem queueItem, string reason)
    {
        try
        {
            using var scope = ScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<IDbContext>();
            var payrunJobRepository = scope.ServiceProvider.GetRequiredService<IPayrunJobRepository>();

            var job = await payrunJobRepository.GetAsync(
                dbContext, queueItem.TenantId, queueItem.PayrunJobId);

            if (job != null && (job.JobStatus != queueItem.JobInvocation.JobStatus || job.JobEnd == null))
            {
                job.JobStatus = queueItem.JobInvocation.JobStatus;
                job.JobEnd = Date.Now;
                job.ErrorMessage = reason;

                var duration = job.JobEnd.Value - job.JobStart;
                job.Message = $"Job completed in {duration.ToReadableString()}: {reason}";

                await payrunJobRepository.UpdateAsync(dbContext, queueItem.TenantId, job);

                Log.Debug(
                    $"Marked payrun job {queueItem.PayrunJobId} as completed: {reason}.");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex,
                $"Failed to mark job {queueItem.PayrunJobId} as completed.");
        }
    }

    private static async Task SendJobCompletionWebhookAsync(
        IDbContext dbContext,
        IWebhookDispatchService webhookDispatcher,
        int tenantId,
        PayrunJob payrunJob,
        int userId)
    {
        try
        {
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
