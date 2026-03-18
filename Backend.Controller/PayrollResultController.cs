using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;
// ReSharper disable UnusedParameter.Global

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Payroll results")]
[Route("api/tenants/{tenantId}/payrollresults")]
[TenantAuthorize]
public class PayrollResultController : Api.Controller.PayrollResultController
{

    /// <inheritdoc/>
    public PayrollResultController(ITenantService tenantService, IPayrollResultService payrollResultService,
        IControllerRuntime runtime) :
        base(tenantService, payrollResultService, runtime)
    {
    }

    #region Payroll Results

    /// <summary>
    /// Query payroll results
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The payroll results</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryPayrollResults")]
    public async Task<ActionResult> QueryPayrollResultsAsync(int tenantId, [FromQuery] Query query) =>
        await QueryItemsAsync(tenantId, query);

    /// <summary>
    /// Get a payroll result
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollResultId">The payroll result id</param>
    /// <returns>The payroll layer</returns>
    [HttpGet("{payrollResultId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetPayrollResult")]
    public async Task<ActionResult<ApiObject.PayrollResult>> GetPayrollResultAsync(
        int tenantId, int payrollResultId) =>
        await GetAsync(tenantId, payrollResultId);

    #endregion

    #region Collector Results

    /// <summary>
    /// Query payroll collector results
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollResultId">The payroll result id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The payroll collector results</returns>
    [HttpGet("{payrollResultId}/collectors")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("QueryCollectorResults")]
    public override async Task<ActionResult<ApiObject.CollectorResult[]>> QueryCollectorResultsAsync(
        int tenantId, int payrollResultId, [FromQuery] Query query) =>
        await base.QueryCollectorResultsAsync(tenantId, payrollResultId, query);

    /// <summary>
    /// Query payroll collector custom results
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollResultId">The payroll result id</param>
    /// <param name="collectorResultId">The collector result id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The payroll collector custom results</returns>
    [HttpGet("{payrollResultId}/collectors/{collectorResultId}/custom")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("QueryCollectorCustomResults")]
    public override async Task<ActionResult<ApiObject.CollectorCustomResult[]>> QueryCollectorCustomResultsAsync(
        int tenantId, int payrollResultId, int collectorResultId, [FromQuery] Query query) =>
        await base.QueryCollectorCustomResultsAsync(tenantId, payrollResultId, collectorResultId, query);

    #endregion

    #region Wage Type Results

    /// <summary>
    /// Query payroll wage type results
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollResultId">The payroll result id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The payroll wage type results</returns>
    [HttpGet("{payrollResultId}/wagetypes")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("QueryWageTypeResults")]
    public override async Task<ActionResult<ApiObject.WageTypeResult[]>> QueryWageTypeResultsAsync(
        int tenantId, int payrollResultId, [FromQuery] Query query) =>
        await base.QueryWageTypeResultsAsync(tenantId, payrollResultId, query);

    /// <summary>
    /// Query payroll wage type custom results
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollResultId">The payroll result id</param>
    /// <param name="wageTypeResultId">The wage type result id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The payroll wage type custom results</returns>
    [HttpGet("{payrollResultId}/wagetypes/{wageTypeResultId}/custom")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("QueryWageTypeCustomResults")]
    public override async Task<ActionResult<ApiObject.WageTypeCustomResult[]>> QueryWageTypeCustomResultsAsync(
        int tenantId, int payrollResultId, int wageTypeResultId, [FromQuery] Query query) =>
        await base.QueryWageTypeCustomResultsAsync(tenantId, payrollResultId, wageTypeResultId, query);

    #endregion

    #region Payrun Results

    /// <summary>
    /// Query payroll payrun results
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollResultId">The payroll result id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The payroll payrun results</returns>
    [HttpGet("{payrollResultId}/payruns")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("QueryPayrunResults")]
    public override async Task<ActionResult<ApiObject.PayrunResult[]>> QueryPayrunResultsAsync(
        int tenantId, int payrollResultId, [FromQuery] Query query) =>
        await base.QueryPayrunResultsAsync(tenantId, payrollResultId, query);

    #endregion

    #region Result Values

    /// <summary>
    /// Query payroll result values
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="employeeId">The employee id (default: all)</param>
    /// <param name="divisionId">The division id (default: all)</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The payroll result values</returns>
    [HttpGet("values")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("QueryPayrollResultValues")]
    public override async Task<ActionResult> QueryPayrollResultValuesAsync(int tenantId,
        [FromQuery] int? employeeId, [FromQuery] int? divisionId, [FromQuery] Query query) =>
        await base.QueryPayrollResultValuesAsync(tenantId, employeeId, divisionId, query);

    #endregion

    #region Result Set

    /// <summary>
    /// Query payroll result sets
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The payroll results sets</returns>
    [HttpGet("sets")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("QueryPayrollResultSets")]
    public override async Task<ActionResult<ApiObject.PayrollResultSet[]>> QueryPayrollResultSetsAsync(
        int tenantId, [FromQuery] Query query) =>
        await base.QueryPayrollResultSetsAsync(tenantId, query);

    /// <summary>
    /// Get a payroll result set
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollResultId">The payroll result id</param>
    /// <returns>A payroll results set</returns>
    [HttpGet("sets/{payrollResultId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetPayrollResultSet")]
    public override async Task<ActionResult<ApiObject.PayrollResultSet>> GetPayrollResultSetAsync(
        int tenantId, int payrollResultId) =>
        await base.GetPayrollResultSetAsync(tenantId, payrollResultId);

    #endregion
}
