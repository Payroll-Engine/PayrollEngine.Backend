using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the payrun jobs
/// </summary>
public abstract class PayrunJobController : RepositoryChildObjectController<ITenantService, IPayrunJobService,
    ITenantRepository, IPayrunJobRepository,
    Tenant, PayrunJob, ApiObject.PayrunJob>
{
    public IWebhookDispatchService WebhookDispatcher { get; }
    private PayrunJobServiceSettings ServiceSettings => Service.Settings;

    protected PayrunJobController(ITenantService tenantService, IPayrunJobService payrunJobService,
        IWebhookDispatchService webhookDispatcher, IControllerRuntime runtime) :
        base(tenantService, payrunJobService, runtime, new PayrunJobMap())
    {
        WebhookDispatcher = webhookDispatcher ?? throw new ArgumentNullException(nameof(webhookDispatcher));
    }

    public virtual async Task<ActionResult> QueryEmployeePayrunJobsAsync(int tenantId, int employeeId, Query query)
    {
        query ??= new();
        query.Result ??= QueryResultType.Items;
        switch (query.Result)
        {
            case QueryResultType.Items:
                var items = await QueryEmployeeJobsAsync(tenantId, employeeId, query);
                return items.IsValidResult() ? Ok(items.Value) : items.Result;
            case QueryResultType.Count:
                var count = await QueryEmployeeJobsCountAsync(tenantId, employeeId, query);
                return count.IsValidResult() ? Ok(count.Value) : count.Result;
            case QueryResultType.ItemsWithCount:
                items = await QueryEmployeeJobsAsync(tenantId, employeeId, query);
                count = await QueryEmployeeJobsCountAsync(tenantId, employeeId, query);
                return items.IsValidResult() && count.IsValidResult() ?
                    Ok(new QueryResult<ApiObject.PayrunJob>(items.Value, count.Value)) : items.Result;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async Task<ActionResult<ApiObject.PayrunJob[]>> QueryEmployeeJobsAsync(int tenantId,
        int employeeId, Query query)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }

        return Map.ToApi(await Service.QueryEmployeePayrunJobsAsync(Runtime.DbContext, tenantId, employeeId, query));
    }

    private async Task<ActionResult<long>> QueryEmployeeJobsCountAsync(int tenantId, int employeeId, Query query)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }

        return await Service.QueryEmployeePayrunJobsCountAsync(Runtime.DbContext, tenantId, employeeId, query);
    }

    /// <summary>
    /// Start a new payrun job
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="jobInvocation">The payrun jobs to add</param>
    /// <returns>The started payrun job</returns>
    public virtual async Task<ActionResult<ApiObject.PayrunJob>> StartPayrunJobAsync(int tenantId, ApiObject.PayrunJobInvocation jobInvocation)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        // tenant
        var tenant = await ParentService.GetAsync(Runtime.DbContext, tenantId);
        if (tenant == null)
        {
            return BadRequest($"Unknown tenant with id {tenantId}");
        }

        // user
        if (jobInvocation.UserId <= 0 || !await ServiceSettings.UserRepository.ExistsAsync(
                Runtime.DbContext, tenantId, jobInvocation.UserId))
        {
            return BadRequest($"Unknown user with id {jobInvocation.UserId}");
        }

        // payrun
        if (jobInvocation.PayrunId <= 0 || !await ServiceSettings.PayrunRepository.ExistsAsync(
                Runtime.DbContext, tenantId, jobInvocation.PayrunId))
        {
            return BadRequest($"Unknown payrun with id {jobInvocation.PayrunId}");
        }
        var payrun = await ServiceSettings.PayrunRepository.GetAsync(Runtime.DbContext, tenantId, jobInvocation.PayrunId);
        if (payrun == null || payrun.Status != ObjectStatus.Active)
        {
            return BadRequest($"Inactive payrun with id {jobInvocation.PayrunId}");
        }

        // payroll
        var payrollId = payrun.PayrollId;
        if (!await ServiceSettings.PayrollRepository.ExistsAsync(Runtime.DbContext, tenantId, payrollId))
        {
            return BadRequest($"Unknown payroll with id {payrollId}");
        }

        // legal payrun jobs: check open draft jobs
        // active jobs within the same derived-payroll and job status draft or final
        if (string.IsNullOrWhiteSpace(jobInvocation.Forecast))
        {
            var query = QueryFactory.NewEqualFilterQuery(new Dictionary<string, object>
            {
                {nameof(ApiObject.PayrunJob.Status), (int)ObjectStatus.Active},
                {nameof(ApiObject.PayrunJob.PayrollId), payrollId},
                {nameof(ApiObject.PayrunJob.JobStatus), (int)PayrunJobStatus.Draft}
            });
            var openJobs = await ServiceSettings.PayrunJobRepository.QueryAsync(Runtime.DbContext, tenantId, query);
            if (openJobs.Any())
            {
                // TODO payrun job status test
                //  return BadRequest($"Payroll with id {jobInvocation.PayrollId} has already a payrun job with status {PayrunJobStatus.Draft}");
            }
        }

        // processor
        try
        {
            // settings
            var serverConfiguration = Runtime.Configuration.GetConfiguration<PayrollServerConfiguration>();

            using var processor = new PayrunProcessor(
                tenant,
                payrun,
                new()
                {
                    DbContext = Runtime.DbContext,
                    CalendarRepository = ServiceSettings.CalendarRepository,
                    UserRepository = ServiceSettings.UserRepository,
                    DivisionRepository = ServiceSettings.DivisionRepository,
                    TaskRepository = ServiceSettings.TaskRepository,
                    LogRepository = ServiceSettings.LogRepository,
                    EmployeeRepository = ServiceSettings.EmployeeRepository,
                    CaseRepository = ServiceSettings.CaseRepository,
                    GlobalCaseValueRepository = ServiceSettings.GlobalCaseValueRepository,
                    NationalCaseValueRepository = ServiceSettings.NationalCaseValueRepository,
                    CompanyCaseValueRepository = ServiceSettings.CompanyCaseValueRepository,
                    EmployeeCaseValueRepository = ServiceSettings.EmployeeCaseValueRepository,
                    PayrunRepository = ServiceSettings.PayrunRepository,
                    PayrunJobRepository = ServiceSettings.PayrunJobRepository,
                    CollectorRepository = ServiceSettings.CollectorRepository,
                    WageTypeRepository = ServiceSettings.WageTypeRepository,
                    RegulationLookupSetRepository = ServiceSettings.RegulationLookupSetRepository,
                    RegulationRepository = ServiceSettings.RegulationRepository,
                    RegulationShareRepository = ServiceSettings.RegulationShareRepository,
                    PayrollRepository = ServiceSettings.PayrollRepository,
                    PayrollResultRepository = ServiceSettings.PayrollResultRepository,
                    PayrollConsolidatedResultRepository = ServiceSettings.PayrollConsolidatedResultRepository,
                    PayrollResultSetRepository = ServiceSettings.PayrollResultSetRepository,
                    PayrollCalculatorProvider = ServiceSettings.PayrollCalculatorProvider,
                    WebhookDispatchService = WebhookDispatcher,
                    FunctionLogTimeout = serverConfiguration.FunctionLogTimeout,
                    AssemblyCacheTimeout = serverConfiguration.AssemblyCacheTimeout,
                    ScriptProvider = Runtime.ScriptProvider,
                });

            // job
            var domainJobInvocation = new PayrunJobInvocationMap().ToDomain(jobInvocation);
            var payrunJob = await processor.Process(domainJobInvocation);

            stopwatch.Stop();
            Log.Debug($"Created job {payrunJob.Name}: {stopwatch.ElapsedMilliseconds} ms");

            // created resource
            return new CreatedObjectResult(Request.Path, MapDomainToApi(payrunJob));
        }
        catch (PayrunException exception)
        {
            Log.Error(exception, exception.GetBaseMessage());
            return BadRequest($"Error in payrun {jobInvocation.PayrunId}: {exception.GetBaseMessage()}");
        }
        catch (Exception exception)
        {
            Log.Error(exception, exception.GetBaseMessage());
            return InternalServerError(exception);
        }
    }

    /// <summary>
    /// Get status of a payrun job
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrunJobId">The payrun job id</param>
    /// <returns>The payrun job status</returns>
    public virtual async Task<ActionResult<string>> GetPayrunJobStatusAsync(int tenantId, int payrunJobId)
    {
        // tenant
        var tenant = await ParentService.GetAsync(Runtime.DbContext, tenantId);
        if (tenant == null)
        {
            return BadRequest($"Unknown tenant with id {tenantId}");
        }

        // payrun job
        var payrunJob = await Service.GetAsync(Runtime.DbContext, tenantId, payrunJobId);
        if (payrunJob == null)
        {
            return BadRequest($"Unknown payrun job with id {payrunJobId}");
        }

        return Enum.GetName(typeof(PayrunJobStatus), payrunJob.JobStatus);
    }

    /// <summary>
    /// Change the status of a payrun job
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrunJobId">The payrun job id</param>
    /// <param name="jobStatus">The new payrun job status</param>
    /// <param name="userId">The user id</param>
    /// <param name="reason">The change reason</param>
    /// <param name="patchMode">Use the patch mode</param>
    public virtual async Task<IActionResult> ChangePayrunJobStatusAsync(int tenantId, int payrunJobId,
        PayrunJobStatus jobStatus, int userId, string reason, bool patchMode)
    {
        // tenant
        var tenant = await ParentService.GetAsync(Runtime.DbContext, tenantId);
        if (tenant == null)
        {
            return BadRequest($"Unknown tenant with id {tenantId}");
        }

        // payrun job
        var payrunJob = await Service.GetAsync(Runtime.DbContext, tenantId, payrunJobId);
        if (payrunJob == null)
        {
            return BadRequest($"Unknown payrun job with id {payrunJobId}");
        }
        if (payrunJob.Status != ObjectStatus.Active)
        {
            return BadRequest($"Inactive payrun job with id {payrunJobId}");
        }

        // patch mode (no state change validation)
        if (patchMode)
        {
            payrunJob = await ServiceSettings.PayrunJobRepository.PatchPayrunJobStatusAsync(
                Runtime.DbContext, tenantId, payrunJobId, jobStatus, userId, reason);
            if (payrunJob == null)
            {
                return BadRequest($"Unknown patch payrun job with id {payrunJobId}");
            }
            if (payrunJob.JobStatus != jobStatus)
            {
                return BadRequest($"Status patch from {payrunJob.JobStatus} to {jobStatus} failed in payrun job with id {payrunJobId}");
            }
            return Ok();
        }

        // keep status
        var oldJobStatus = payrunJob.JobStatus;
        if (oldJobStatus == jobStatus)
        {
            return Ok();
        }

        // change status
        if (payrunJob.JobStatus.IsFinal())
        {
            return BadRequest($"Finalized payrun job with status {payrunJob.JobStatus} can not be changed in payrun job with id {payrunJobId}");
        }
        if (!payrunJob.JobStatus.IsValidStateChange(jobStatus))
        {
            return BadRequest($"Invalid payrun job status change from {payrunJob.JobStatus} to {jobStatus} in payrun job with id {payrunJobId}");
        }

        // update payrun job
        payrunJob.JobStatus = jobStatus;
        var updatedPayrunJob = await UpdateAsync(tenantId, Map.ToApi(payrunJob));
        if (updatedPayrunJob.Value == null)
        {
            return updatedPayrunJob.Result;
        }

        // webhook payrun job process
        if (oldJobStatus.IsWebhookProcessChange(jobStatus))
        {
            var json = DefaultJsonSerializer.Serialize(payrunJob);
            await WebhookDispatcher.SendMessageAsync(Runtime.DbContext, tenantId,
                new()
                {
                    Action = WebhookAction.PayrunJobProcess,
                    RequestMessage = json
                },
                userId: userId);
        }

        // webhook payrun job finished
        if (oldJobStatus.IsWebhookFinishChange(jobStatus))
        {
            var json = DefaultJsonSerializer.Serialize(payrunJob);
            await WebhookDispatcher.SendMessageAsync(Runtime.DbContext, tenantId,
                new()
                {
                    Action = WebhookAction.PayrunJobFinish,
                    RequestMessage = json
                },
                userId: userId);
        }

        return Ok();
    }
}