using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Payroll consolidated results")]
[Route("api/tenants/{tenantId}/payrollresults/consolidated")]
public class PayrollConsolidatedResultController : Api.Controller.PayrollConsolidatedResultController
{
    /// <inheritdoc/>
    public PayrollConsolidatedResultController(ITenantService tenantService, IPayrollConsolidatedResultService payrollResultService,
        IControllerRuntime runtime) :
        base(tenantService, payrollResultService, runtime)
    {
    }

    /// <summary>
    /// Query consolidated payroll results
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="employeeId">The employee id</param>
    /// <param name="periodStart">Period start date</param>
    /// <param name="periodEnd">Period end date</param>
    /// <param name="divisionId">The division id</param>
    /// <param name="forecast">The forecast name</param>
    /// <param name="jobStatus">The payrun job status</param>
    /// <param name="tags">The result tags</param>
    /// <returns>The period collector results</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetConsolidatedPayrollResult")]
    public async Task<ActionResult<ApiObject.ConsolidatedPayrollResult>> GetConsolidatedPayrollResultAsync(int tenantId,
        [Required] int employeeId, [Required][FromQuery] DateTime periodStart, [Required][FromQuery] DateTime periodEnd,
        [FromQuery] int? divisionId, [FromQuery] string forecast, [FromQuery] PayrunJobStatus? jobStatus,
        [FromQuery] string[] tags)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }

        var results = await Service.GetPayrollResultAsync(Runtime.DbContext,
            new()
            {
                TenantId = tenantId,
                EmployeeId = employeeId,
                DivisionId = divisionId,
                Period = new(periodStart.ToUtc(), periodEnd.ToUtc()),
                Forecast = forecast,
                Tags = tags,
                JobStatus = jobStatus
            });
        return new ConsolidatedPayrollResultMap().ToApi(results);
    }

    /// <summary>
    /// Get consolidated collector results
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="employeeId">The employee id</param>
    /// <param name="periodStarts">Period start dates</param>
    /// <param name="divisionId">The division id</param>
    /// <param name="collectorNames">The collector names</param>
    /// <param name="forecast">The forecast name</param>
    /// <param name="jobStatus">The payrun job status</param>
    /// <param name="tags">The result tags</param>
    /// <param name="evaluationDate">The evaluation date (default: UTC now)</param>
    /// <returns>The period collector results</returns>
    [HttpGet("collectors")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetConsolidatedCollectorResults")]
    public async Task<ActionResult<ApiObject.CollectorResult[]>> GetConsolidatedCollectorResultsAsync(int tenantId,
        [Required] int employeeId, [Required][FromQuery] DateTime[] periodStarts,
        [FromQuery] int? divisionId, [FromQuery] string[] collectorNames, [FromQuery] string forecast,
        [FromQuery] PayrunJobStatus? jobStatus, [FromQuery] string[] tags, [FromQuery] DateTime? evaluationDate)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }

        // periods
        if (periodStarts == null || !periodStarts.Any())
        {
            return BadRequest("Missing consolidated period start dates");
        }

        var results = await Service.GetCollectorResultsAsync(Runtime.DbContext,
            new()
            {
                TenantId = tenantId,
                EmployeeId = employeeId,
                DivisionId = divisionId,
                CollectorNames = collectorNames,
                PeriodStarts = periodStarts.ToList(),
                Forecast = forecast,
                JobStatus = jobStatus,
                Tags = tags,
                EvaluationDate = evaluationDate
            });
        return new CollectorResultMap().ToApi(results);
    }

    /// <summary>
    /// Query consolidated wage type results
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="employeeId">The employee id</param>
    /// <param name="periodStarts">Period start dates</param>
    /// <param name="divisionId">The division id</param>
    /// <param name="wageTypeNumbers">The wage type numbers</param>
    /// <param name="forecast">The forecast name</param>
    /// <param name="jobStatus">The payrun job status</param>
    /// <param name="tags">The result tags</param>
    /// <param name="evaluationDate">The evaluation date (default: UTC now)</param>
    /// <returns>The period wage type results</returns>
    [HttpGet("wagetypes")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetConsolidatedWageTypeResults")]
    public async Task<ActionResult<ApiObject.WageTypeResult[]>> GetConsolidatedWageTypeResultsAsync(int tenantId,
        [Required] int employeeId, [Required][FromQuery] DateTime[] periodStarts,
        [FromQuery] int? divisionId, [FromQuery] decimal[] wageTypeNumbers, [FromQuery] string forecast,
        [FromQuery] string[] tags, [FromQuery] DateTime? evaluationDate, [FromQuery] PayrunJobStatus? jobStatus)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }

        // periods
        if (periodStarts == null || !periodStarts.Any())
        {
            return BadRequest("Missing consolidated period start dates");
        }

        var results = await Service.GetWageTypeResultsAsync(Runtime.DbContext,
            new()
            {
                TenantId = tenantId,
                EmployeeId = employeeId,
                DivisionId = divisionId,
                WageTypeNumbers = wageTypeNumbers,
                PeriodStarts = periodStarts.ToList(),
                Forecast = forecast,
                JobStatus = jobStatus,
                Tags = tags,
                EvaluationDate = evaluationDate
            });
        return new WageTypeResultMap().ToApi(results);
    }

    /// <summary>
    /// Query consolidated payrun results
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="employeeId">The employee id</param>
    /// <param name="periodStarts">Period start dates</param>
    /// <param name="divisionId">The division id</param>
    /// <param name="resultNames">The result names</param>
    /// <param name="forecast">The forecast name</param>
    /// <param name="jobStatus">The payrun job status</param>
    /// <param name="tags">The result tags</param>
    /// <param name="evaluationDate">The evaluation date (default: UTC now)</param>
    /// <returns>The period payrun results</returns>
    [HttpGet("payruns")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetConsolidatedPayrunResults")]
    public async Task<ActionResult<ApiObject.PayrunResult[]>> GetConsolidatedPayrunResultsAsync(int tenantId,
        [Required] int employeeId, [Required][FromQuery] DateTime[] periodStarts,
        [FromQuery] int? divisionId, [FromQuery] string[] resultNames, [FromQuery] string forecast,
        [FromQuery] string[] tags, [FromQuery] DateTime? evaluationDate, [FromQuery] PayrunJobStatus? jobStatus)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }

        // periods
        if (periodStarts == null || !periodStarts.Any())
        {
            return BadRequest("Missing consolidated period start dates");
        }

        var results = await Service.GetPayrunResultsAsync(Runtime.DbContext,
            new()
            {
                TenantId = tenantId,
                EmployeeId = employeeId,
                DivisionId = divisionId,
                ResultNames = resultNames,
                PeriodStarts = periodStarts.ToList(),
                Forecast = forecast,
                JobStatus = jobStatus,
                Tags = tags,
                EvaluationDate = evaluationDate
            });
        return new PayrunResultMap().ToApi(results);
    }
}