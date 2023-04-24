using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
public class PayrollResultController : Api.Controller.PayrollResultController
{
    /// <summary>
    /// The payroll service
    /// </summary>
    public IPayrollService PayrollService { get; }

    /// <summary>
    /// The payrun service
    /// </summary>
    public IPayrunService PayrunService { get; }

    /// <inheritdoc/>
    public PayrollResultController(ITenantService tenantService, IPayrollResultService payrollResultService,
        IPayrollService payrollService, IPayrunService payrunService, IControllerRuntime runtime) :
        base(tenantService, payrollResultService, runtime)
    {
        PayrollService = payrollService ?? throw new ArgumentNullException(nameof(payrollService));
        PayrunService = payrunService ?? throw new ArgumentNullException(nameof(payrunService));
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
    public async Task<ActionResult> QueryPayrollResultsAsync(int tenantId, [FromQuery] Query query)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await QueryItemsAsync(tenantId, query);
    }

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
    public async Task<ActionResult<ApiObject.PayrollResult>> GetPayrollResultAsync(int tenantId, int payrollResultId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await GetAsync(tenantId, payrollResultId);
    }

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
    public async Task<ActionResult<ApiObject.CollectorResult[]>> QueryCollectorResultsAsync(int tenantId, int payrollResultId,
        [FromQuery] Query query)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        var results = await Service.QueryCollectorResultsAsync(Runtime.DbContext, payrollResultId, query);
        return new CollectorResultMap().ToApi(results);
    }

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
    public async Task<ActionResult<ApiObject.CollectorCustomResult[]>> QueryCollectorCustomResultsAsync(int tenantId,
        int payrollResultId, int collectorResultId, [FromQuery] Query query)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        var results = await Service.QueryCollectorCustomResultsAsync(Runtime.DbContext, collectorResultId, query);
        return new CollectorCustomResultMap().ToApi(results);
    }

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
    public async Task<ActionResult<ApiObject.WageTypeResult[]>> QueryWageTypeResultsAsync(int tenantId, int payrollResultId,
        [FromQuery] Query query)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        var results = await Service.QueryWageTypeResultsAsync(Runtime.DbContext, payrollResultId, query);
        return new WageTypeResultMap().ToApi(results);
    }

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
    public async Task<ActionResult<ApiObject.WageTypeCustomResult[]>> QueryWageTypeCustomResultsAsync(int tenantId,
        int payrollResultId, int wageTypeResultId, [FromQuery] Query query)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        var results = await Service.QueryWageTypeCustomResultsAsync(Runtime.DbContext, wageTypeResultId, query);
        return new WageTypeCustomResultMap().ToApi(results);
    }

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
    public async Task<ActionResult<ApiObject.PayrunResult[]>> QueryPayrunResultsAsync(int tenantId, int payrollResultId,
        [FromQuery] Query query)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        var results = await Service.QueryPayrunResultsAsync(Runtime.DbContext, payrollResultId, query);
        return new PayrunResultMap().ToApi(results);
    }

    #endregion

    #region Result Values

    /// <summary>
    /// Query payroll result values
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="employeeId">The employee id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The payroll result values</returns>
    [HttpGet("values")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("QueryPayrollResultValues")]
    public override async Task<ActionResult> QueryPayrollResultValuesAsync(int tenantId, [Required] int employeeId,
        [FromQuery] Query query)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await base.QueryPayrollResultValuesAsync(tenantId, employeeId, query);
    }

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
    public async Task<ActionResult<ApiObject.PayrollResultSet[]>> QueryPayrollResultSetsAsync(int tenantId,
        [FromQuery] Query query)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }

        var resultSets = await Service.QueryResultSetsAsync(Runtime.DbContext, tenantId, query);
        return new PayrollResultSetMap().ToApi(resultSets);
    }

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
    public async Task<ActionResult<ApiObject.PayrollResultSet>> GetPayrollResultSetAsync(int tenantId, int payrollResultId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        var resultSet = await Service.GetResultSetAsync(Runtime.DbContext, tenantId, payrollResultId);
        if (resultSet == null)
        {
            return NotFound($"Payroll result with id {payrollResultId} was not found");
        }
        return new PayrollResultSetMap().ToApi(resultSet);
    }

    #endregion
}