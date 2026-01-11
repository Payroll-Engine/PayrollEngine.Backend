using System;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Application;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Backend.Server;

/// <summary>
/// Background service that processes payrun jobs from the queue
/// </summary>
public class PayrunJobWorkerService : BackgroundService
{
    private readonly IPayrunJobQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>
    /// Initializes a new instance of the PayrunJobWorkerService
    /// </summary>
    public PayrunJobWorkerService(
        IPayrunJobQueue queue,
        IServiceScopeFactory scopeFactory)
    {
        _queue = queue ?? throw new ArgumentNullException(nameof(queue));
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.Information("Payrun Job Worker Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            PayrunJobQueueItem queueItem = null;
            try
            {
                // Wait for a job to be available
                queueItem = await _queue.DequeueAsync(stoppingToken);

                Log.Information(
                    $"Processing payrun job {queueItem.PayrunJobId} for tenant {queueItem.TenantId}");

                await ProcessJobAsync(queueItem, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Graceful shutdown - mark in-progress job as aborted if exists
                if (queueItem != null)
                {
                    await MarkJobAbortedAsync(queueItem, "Service shutdown");
                }
                break;
            }
            catch (Exception ex)
            {
                Log.Error(ex,
                    $"Error processing payrun job {queueItem?.PayrunJobId}: {ex.Message}");

                if (queueItem != null)
                {
                    await MarkJobAbortedAsync(queueItem, ex.Message);
                }
            }
        }

        Log.Information("Payrun Job Worker Service stopped");
    }

    private async Task ProcessJobAsync(
        PayrunJobQueueItem queueItem,
        CancellationToken cancellationToken)
    {
        // Create a new scope for each job to get fresh scoped services
        using var scope = _scopeFactory.CreateScope();
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

        Log.Information(
            $"Completed payrun job {payrunJob.Id} with status {payrunJob.JobStatus}");

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
            using var scope = _scopeFactory.CreateScope();
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
                    $"Marked payrun job {queueItem.PayrunJobId} as aborted: {reason}");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex,
                $"Failed to mark job {queueItem.PayrunJobId} as aborted");
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

            var json = PayrollEngine.Serialization.DefaultJsonSerializer.Serialize(payrunJob);

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
            Log.Warning(ex, $"Failed to send webhook for job {payrunJob.Id}");
        }
    }
}
