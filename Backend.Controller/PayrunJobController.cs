using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Payrun jobs")]
[Route("api/tenants/{tenantId}/payruns/jobs")]
[TenantAuthorize]
public class PayrunJobController : Api.Controller.PayrunJobController
{
    /// <inheritdoc/>
    public PayrunJobController(ITenantService tenantService, IPayrunJobService payrunJobService,
        IWebhookDispatchService webhookDispatcher, IPayrunJobQueue payrunJobQueue,
        IPayrunPreviewService payrunPreviewService, IControllerRuntime runtime) :
        base(tenantService, payrunJobService, webhookDispatcher, payrunJobQueue, payrunPreviewService, runtime)
    {
    }

    /// <summary>
    /// Query payrun jobs
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The tenant payrun jobs</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryPayrunJobs")]
    public async Task<ActionResult> QueryPayrunJobsAsync(int tenantId, [FromQuery] Query query) =>
        await QueryItemsAsync(tenantId, query);

    /// <summary>
    /// Get employee payrun jobs
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="employeeId">The employee id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The employee payrun jobs</returns>
    [HttpGet("employees/{employeeId}")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryEmployeePayrunJobs")]
    public override async Task<ActionResult> QueryEmployeePayrunJobsAsync(
        int tenantId, int employeeId, [FromQuery] Query query) =>
        await base.QueryEmployeePayrunJobsAsync(tenantId, employeeId, query);

    /// <summary>
    /// Get a payrun job
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrunJobId">The id of the payrun job</param>
    /// <returns>The payrun job</returns>
    [HttpGet("{payrunJobId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetPayrunJob")]
    public async Task<ActionResult<ApiObject.PayrunJob>> GetPayrunJobAsync(
        int tenantId, int payrunJobId) =>
        await GetAsync(tenantId, payrunJobId);

    /// <summary>
    /// Start a new payrun job (asynchronously).
    /// The job is queued for background processing and returns immediately.
    /// Use the Location header to poll for job status.
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="jobInvocation">The payrun job invocation</param>
    /// <returns>HTTP 202 Accepted with the payrun job and Location header for status polling</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("StartPayrunJob")]
    [EnableRateLimiting("PayrunJobStart")]
    public override async Task<ActionResult<ApiObject.PayrunJob>> StartPayrunJobAsync(
        int tenantId, ApiObject.PayrunJobInvocation jobInvocation) =>
        await base.StartPayrunJobAsync(tenantId, jobInvocation);

    /// <summary>
    /// Preview a payrun job for a single employee (synchronous).
    /// Returns calculation results as a PayrollResultSet without persisting to the database.
    /// <para>Restriction: preview results are not persisted, so wage type expressions
    /// that query historical results (e.g. GetPeriodWageTypeResults) will not find
    /// results from previous preview invocations. Only already persisted payrun job
    /// results are available during preview calculation.</para>
    /// <para>The preview accepts any <c>RetroPayMode</c> to match production behavior.
    /// If retroactive calculation is actually triggered (mutations detected in prior periods),
    /// the endpoint returns HTTP 422 Unprocessable Entity.</para>
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="jobInvocation">The payrun job invocation with exactly one employee identifier</param>
    /// <returns>The payroll result set containing wage type results, collector results, and payrun results</returns>
    [HttpPost("preview")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("PreviewPayrunJob")]
    public override async Task<ActionResult<ApiObject.PayrollResultSet>> PreviewPayrunJobAsync(
        int tenantId, ApiObject.PayrunJobInvocation jobInvocation) =>
        await base.PreviewPayrunJobAsync(tenantId, jobInvocation);

    /// <summary>
    /// Get status of a payrun job
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrunJobId">The payrun job id</param>
    /// <returns>The payrun job status</returns>
    [HttpGet("{payrunJobId}/status")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetPayrunJobStatus")]
    public override async Task<ActionResult<string>> GetPayrunJobStatusAsync(
        int tenantId, int payrunJobId) =>
        await base.GetPayrunJobStatusAsync(tenantId, payrunJobId);

    /// <summary>
    /// Change the status of a payrun job
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrunJobId">The payrun job id</param>
    /// <param name="jobStatus">The new payrun job status</param>
    /// <param name="userId">The user id</param>
    /// <param name="reason">The change reason</param>
    /// <param name="patchMode">Use the patch mode</param>
    /// <returns>The updated payrun job</returns>
    [HttpPost("{payrunJobId}/status")]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("ChangePayrunJobStatus")]
    public override async Task<IActionResult> ChangePayrunJobStatusAsync(
        int tenantId, int payrunJobId, [FromBody] PayrunJobStatus jobStatus,
        [Required] int userId, [Required] string reason, bool patchMode) =>
        await base.ChangePayrunJobStatusAsync(tenantId, payrunJobId, jobStatus, userId, reason, patchMode);

    /// <summary>
    /// Delete a payrun jobs including all payroll results
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrunJobId">The id of the payrun jobs</param>
    [HttpDelete("{payrunJobId}")]
    [ApiOperationId("DeletePayrunJob")]
    public async Task<IActionResult> DeletePayrunJobAsync(int tenantId, int payrunJobId) =>
        await DeleteAsync(tenantId, payrunJobId);

    #region Attributes

    /// <summary>
    /// Get a payrun job attribute
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrunJobId">The id of the payrun job</param>
    /// <param name="attributeName">The attribute name</param>
    /// <returns>The attribute value as JSON</returns>
    [HttpGet("{payrunJobId}/attributes/{attributeName}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetPayrunJobAttribute")]
    public virtual async Task<ActionResult<string>> GetPayrunJobAttributeAsync(
        int tenantId, int payrunJobId, string attributeName) =>
        await GetAttributeAsync(payrunJobId, attributeName);

    /// <summary>
    /// Set a payrun job attribute
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrunJobId">The id of the payrun job</param>
    /// <param name="attributeName">The attribute name</param>
    /// <param name="value">The attribute value as JSON</param>
    /// <returns>The current attribute value as JSON</returns>
    [HttpPost("{payrunJobId}/attributes/{attributeName}")]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("SetPayrunJobAttribute")]
    public virtual async Task<ActionResult<string>> SetPayrunJobAttributeAsync(
        int tenantId, int payrunJobId, string attributeName, [FromBody] string value) =>
        await SetAttributeAsync(payrunJobId, attributeName, value);

    /// <summary>
    /// Delete a payrun job attribute
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrunJobId">The id of the payrun job</param>
    /// <param name="attributeName">The attribute name</param>
    /// <returns>True if the attribute was deleted</returns>
    [HttpDelete("{payrunJobId}/attributes/{attributeName}")]
    [ApiOperationId("DeletePayrunJobAttribute")]
    public virtual async Task<ActionResult<bool>> DeletePayrunJobAttributeAsync(
        int tenantId, int payrunJobId, string attributeName) =>
        await DeleteAttributeAsync(payrunJobId, attributeName);

    #endregion

}
