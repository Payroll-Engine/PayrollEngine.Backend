using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Employee case values")]
[Route("api/tenants/{tenantId}/employees/{employeeId}/cases")]
public class EmployeeCaseValueController : Api.Controller.EmployeeCaseValueController
{
    /// <inheritdoc/>
    public EmployeeCaseValueController(IEmployeeService employeeService, IEmployeeCaseValueService caseValueService,
        IPayrollService payrollsService, IRegulationService regulationService, ILookupSetService lookupSetService, IControllerRuntime runtime) :
        base(employeeService, caseValueService, payrollsService, regulationService, lookupSetService, runtime)
    {
    }

    /// <summary>
    /// Query employee case values
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="employeeId">The employee id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The employee case values</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryEmployeeCaseValues")]
    public async Task<ActionResult> QueryEmployeeCaseValuesAsync(int tenantId, int employeeId, [FromQuery] CaseValueQuery query)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }

        // employee with tenant check
        var employee = await ParentService.GetAsync(Runtime.DbContext, tenantId, employeeId);
        if (employee == null)
        {
            return BadRequest($"Unknown employee with id {employeeId}");
        }

        return await QueryItemsAsync(employeeId, query);
    }

    /// <summary>
    /// Get employee case value slots
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="employeeId">The employee id</param>
    /// <param name="caseFieldName">The case field name</param>
    /// <returns>The case value slots</returns>
    [HttpGet("slots")]
    [OkResponse]
    [ApiOperationId("GetEmployeeCaseValueSlots")]
    public async Task<ActionResult<IEnumerable<string>>> GetEmployeeCaseValueSlotsAsync(int tenantId, int employeeId,
        [Required] string caseFieldName)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }

        // employee check
        if (!await ParentService.ExistsAsync(Runtime.DbContext, employeeId))
        {
            return BadRequest($"Unknown employee with id {employeeId}");
        }

        return Ok(await GetCaseValueSlotsAsync(employeeId, caseFieldName));
    }

    /// <summary>
    /// Get an employee case value
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="employeeId">The employee id</param>
    /// <param name="caseValueId">The employee case value id</param>
    /// <returns>The case value</returns>
    [HttpGet("{caseValueId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetEmployeeCaseValue")]
    public async Task<ActionResult<ApiObject.CaseValue>> GetEmployeeCaseValueAsync(int tenantId, int employeeId, int caseValueId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }

        // employee check
        if (!await ParentService.ExistsAsync(Runtime.DbContext, employeeId))
        {
            return BadRequest($"Unknown employee with id {employeeId}");
        }

        return await GetAsync(employeeId, caseValueId);
    }
}