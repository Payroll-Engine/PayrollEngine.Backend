using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the payrun jobs
/// </summary>
public abstract class PayrunJobController(ITenantService tenantService, IPayrunJobService payrunJobService,
        IWebhookDispatchService webhookDispatcher, IPayrunJobQueue payrunJobQueue,
        IPayrunPreviewService payrunPreviewService, IControllerRuntime runtime)
    : RepositoryChildObjectController<ITenantService, IPayrunJobService,
    ITenantRepository, IPayrunJobRepository,
    Tenant, PayrunJob, ApiObject.PayrunJob>(tenantService, payrunJobService, runtime, new PayrunJobMap())
{
    private IWebhookDispatchService WebhookDispatcher { get; } = webhookDispatcher ?? throw new ArgumentNullException(nameof(webhookDispatcher));
    private IPayrunJobQueue PayrunJobQueue { get; } = payrunJobQueue ?? throw new ArgumentNullException(nameof(payrunJobQueue));
    private IPayrunPreviewService PayrunPreviewService { get; } = payrunPreviewService ?? throw new ArgumentNullException(nameof(payrunPreviewService));
    private PayrunJobServiceSettings ServiceSettings => Service.Settings;

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
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if (authResult != null)
        {
            return authResult;
        }

        return Map.ToApi(await Service.QueryEmployeePayrunJobsAsync(Runtime.DbContext, tenantId, employeeId, query));
    }

    private async Task<ActionResult<long>> QueryEmployeeJobsCountAsync(int tenantId, int employeeId, Query query)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if (authResult != null)
        {
            return authResult;
        }

        return await Service.QueryEmployeePayrunJobsCountAsync(Runtime.DbContext, tenantId, employeeId, query);
    }

    /// <summary>
    /// Start a new payrun job (asynchronously).
    /// The job is queued for background processing and returns immediately with HTTP 202 Accepted.
    /// Use the Location header to poll for job status.
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="jobInvocation">The payrun job invocation</param>
    /// <returns>HTTP 202 Accepted with the payrun job and Location header for status polling</returns>
    public virtual async Task<ActionResult<ApiObject.PayrunJob>> StartPayrunJobAsync(int tenantId, ApiObject.PayrunJobInvocation jobInvocation)
    {
        // tenant
        var tenant = await ParentService.GetAsync(Runtime.DbContext, tenantId);
        if (tenant == null)
        {
            return BadRequest($"Unknown tenant with id {tenantId}");
        }

        // resolve user by identifier
        if (string.IsNullOrWhiteSpace(jobInvocation.UserIdentifier))
        {
            return BadRequest("UserIdentifier is required.");
        }
        var users = (await ServiceSettings.UserRepository.QueryAsync(
            Runtime.DbContext, tenantId, QueryFactory.NewIdentifierQuery(jobInvocation.UserIdentifier))).ToList();
        if (users.Count != 1)
        {
            return BadRequest($"Unknown user with identifier {jobInvocation.UserIdentifier}");
        }
        var user = users.First();

        // resolve payrun by name
        if (string.IsNullOrWhiteSpace(jobInvocation.PayrunName))
        {
            return BadRequest("PayrunName is required.");
        }
        var payruns = (await ServiceSettings.PayrunRepository.QueryAsync(
            Runtime.DbContext, tenantId, QueryFactory.NewNameQuery(jobInvocation.PayrunName))).ToList();
        if (payruns.Count != 1)
        {
            return BadRequest($"Unknown payrun with name {jobInvocation.PayrunName}");
        }
        var payrun = payruns.First();
        if (payrun.Status != ObjectStatus.Active)
        {
            return BadRequest($"Inactive payrun {jobInvocation.PayrunName}");
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
                return BadRequest($"Payrun {jobInvocation.PayrunName} has already a payrun job with status {PayrunJobStatus.Draft}");
            }
        }

        try
        {
            // Map API model to domain model
            var domainJobInvocation = new PayrunJobInvocationMap().ToDomain(jobInvocation);

            // Get payroll and division for job creation
            var payroll = await ServiceSettings.PayrollRepository.GetAsync(Runtime.DbContext, tenantId, payrollId);
            var division = await ServiceSettings.DivisionRepository.GetAsync(Runtime.DbContext, tenantId, payroll.DivisionId);

            // Get calendar for period info
            var calendarName = division.Calendar ?? tenant.Calendar;
            Domain.Model.Calendar calendar = null;
            if (!string.IsNullOrWhiteSpace(calendarName))
            {
                calendar = await ServiceSettings.CalendarRepository.GetByNameAsync(Runtime.DbContext, tenantId, calendarName);
            }
            calendar ??= new Domain.Model.Calendar(); // default calendar

            // Get calculator for period info
            var calculator = ServiceSettings.PayrollCalculatorProvider.CreateCalculator(
                tenantId, user.Id,
                CultureInfo.CurrentCulture,
                calendar);

            // Create the payrun job
            var payrunJob = PayrunJobFactory.CreatePayrunJob(
                jobInvocation: domainJobInvocation,
                payrunId: payrun.Id,
                userId: user.Id,
                divisionId: division.Id,
                payrollId: payrollId,
                payrollCalculator: calculator);

            // Initial status is always Draft. CompletedJobStatus (set by TestRunner)
            // is honoured later by the worker after calculation.
            payrunJob.JobStart = Date.Now;
            payrunJob.JobStatus = PayrunJobStatus.Draft;
            payrunJob.Message = "Payrun job queued for background processing";

            // Persist the job to database
            await ServiceSettings.PayrunJobRepository.CreateAsync(Runtime.DbContext, tenantId, payrunJob);

            // Update invocation with job ID
            domainJobInvocation.PayrunJobId = payrunJob.Id;

            // Enqueue for background processing
            try
            {
                await PayrunJobQueue.EnqueueAsync(new PayrunJobQueueItem
                {
                    TenantId = tenantId,
                    PayrunJobId = payrunJob.Id,
                    Tenant = tenant,
                    Payrun = payrun,
                    JobInvocation = domainJobInvocation
                });
            }
            catch (InvalidOperationException queueEx)
            {
                // queue is full - abort the persisted job and return 503
                payrunJob.JobStatus = PayrunJobStatus.Abort;
                payrunJob.JobEnd = Date.Now;
                payrunJob.Message = "Job queue capacity exceeded";
                await ServiceSettings.PayrunJobRepository.UpdateAsync(Runtime.DbContext, tenantId, payrunJob);
                Log.Warning($"Payrun job {payrunJob.Id} rejected: {queueEx.Message}");
                return StatusCode(503, "The server is currently processing too many payrun jobs. Please retry later.");
            }

            Log.Debug($"Queued payrun job {payrunJob.Id} for background processing");

            // Return HTTP 202 Accepted with Location header
            var statusUrl = $"{Request.Path}/{payrunJob.Id}/status";
            return new AcceptedObjectResult(statusUrl, MapDomainToApi(payrunJob));
        }
        catch (PayrunException exception)
        {
            Log.Error(exception, exception.GetBaseMessage());
            return BadRequest($"Error in payrun {jobInvocation.PayrunName}: {exception.GetBaseMessage()}");
        }
        catch (Exception exception)
        {
            Log.Error(exception, exception.GetBaseMessage());
            return InternalServerError(exception);
        }
    }

    /// <summary>
    /// Preview a payrun job for a single employee (synchronous).
    /// Returns calculation results as a <see cref="ApiObject.PayrollResultSet"/> without persisting to the database.
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="jobInvocation">The payrun job invocation with exactly one employee identifier</param>
    /// <returns>The payroll result set containing wage type results, collector results, and payrun results</returns>
    public virtual async Task<ActionResult<ApiObject.PayrollResultSet>> PreviewPayrunJobAsync(
        int tenantId, ApiObject.PayrunJobInvocation jobInvocation)
    {
        // validate: exactly one employee
        if (jobInvocation.EmployeeIdentifiers == null || jobInvocation.EmployeeIdentifiers.Count != 1)
        {
            return BadRequest("Preview requires exactly one employee identifier.");
        }

        // tenant
        var tenant = await ParentService.GetAsync(Runtime.DbContext, tenantId);
        if (tenant == null)
        {
            return BadRequest($"Unknown tenant with id {tenantId}");
        }

        // user identifier required
        if (string.IsNullOrWhiteSpace(jobInvocation.UserIdentifier))
        {
            return BadRequest("UserIdentifier is required.");
        }

        // payrun name required
        if (string.IsNullOrWhiteSpace(jobInvocation.PayrunName))
        {
            return BadRequest("PayrunName is required.");
        }

        try
        {
            var domainJobInvocation = new PayrunJobInvocationMap().ToDomain(jobInvocation);

            // resolve payrun by name
            var payruns = (await ServiceSettings.PayrunRepository.QueryAsync(
                Runtime.DbContext, tenantId, QueryFactory.NewNameQuery(jobInvocation.PayrunName)));
            var payrun = payruns.FirstOrDefault();
            if (payrun == null || payrun.Status != ObjectStatus.Active)
            {
                return BadRequest($"Unknown or inactive payrun {jobInvocation.PayrunName}");
            }

            var domainResultSet = await PayrunPreviewService.PreviewAsync(tenant, payrun, domainJobInvocation);
            var apiResultSet = new PayrollResultSetMap().ToApi(domainResultSet);
            return Ok(apiResultSet);
        }
        catch (PayrunPreviewRetroException exception)
        {
            Log.Warning(exception.GetBaseMessage());
            return UnprocessableEntity($"Preview error in payrun {jobInvocation.PayrunName}: {exception.GetBaseMessage()}");
        }
        catch (PayrunException exception)
        {
            Log.Error(exception, exception.GetBaseMessage());
            return BadRequest($"Preview error in payrun {jobInvocation.PayrunName}: {exception.GetBaseMessage()}");
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
    /// <param name="jobSets">The payrun jobs</param>
    /// <returns>The payrun job status</returns>
    /// <summary>
    /// Import payrun job sets from an external source (archive restore, migration).
    /// Creates the PayrunJob first, then the PayrollResultSets with the new job id.
    /// </summary>
    public virtual async Task<ActionResult<int>> ImportPayrunJobSetsAsync(
        int tenantId, IEnumerable<ApiObject.PayrunJobSet> jobSets)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if (authResult != null)
        {
            return authResult;
        }

        try
        {
            var map = new PayrunJobSetMap();
            var domainJobSets = jobSets.Select(map.ToDomain);
            var count = await Service.ImportPayrunJobSetsAsync(Runtime.DbContext, tenantId, domainJobSets);
            return Ok(count);
        }
        catch (PayrollException exception) when (exception.GetBaseMessage().StartsWith("Import aborted:") &&
                                                  exception.GetBaseMessage().Contains("not found"))
        {
            return UnprocessableEntity(exception.GetBaseMessage());
        }
        catch (PayrollException exception)
        {
            return Conflict(exception.GetBaseMessage());
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

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
                return BadRequest(
                    $"Status patch from {payrunJob.JobStatus} to {jobStatus} failed in payrun job with id {payrunJobId}");
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
            return BadRequest(
                $"Finalized payrun job with status {payrunJob.JobStatus} can not be changed in payrun job with id {payrunJobId}");
        }

        if (!payrunJob.JobStatus.IsValidStateChange(jobStatus))
        {
            return BadRequest(
                $"Invalid payrun job status change from {payrunJob.JobStatus} to {jobStatus} in payrun job with id {payrunJobId}");
        }

        // update payrun job
        payrunJob.JobStatus = jobStatus;
        var updatedPayrunJob = await UpdateAsync(tenantId, Map.ToApi(payrunJob));
        if (updatedPayrunJob.Value == null)
        {
            return updatedPayrunJob.Result;
        }

        // no webhooks
        if (!await WebhookDispatcher.HasWebhooksAsync(Runtime.DbContext, tenantId))
        {
            return Ok();
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