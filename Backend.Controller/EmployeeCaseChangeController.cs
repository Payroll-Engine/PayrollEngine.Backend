using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Employee case changes")]
[Route("api/tenants/{tenantId}/employees/{employeeId}/cases/changes")]
public class EmployeeCaseChangeController : Api.Controller.EmployeeCaseChangeController
{
    /// <inheritdoc/>
    public EmployeeCaseChangeController(IEmployeeService employeeService, IEmployeeCaseChangeService caseChangeService,
         IControllerRuntime runtime) :
        base(employeeService, caseChangeService, runtime)
    {
    }

    /// <summary>
    /// Query employee case changes
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="employeeId">The employee id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The case changes array</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryEmployeeCaseChanges")]
    public async Task<ActionResult> QueryEmployeeCaseChangesAsync(int tenantId, int employeeId, [FromQuery] DomainObject.CaseChangeQuery query)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }

        return await QueryAsync(tenantId, employeeId, query);
    }

    /// <summary>
    /// Get an employee case change
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="employeeId">The employee id</param>
    /// <param name="caseChangeId">The case value change id</param>
    /// <returns>The case value change</returns>
    [HttpGet("{caseChangeId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetEmployeeCaseChange")]
    public async Task<ActionResult<ApiObject.CaseChange>> GetEmployeeCaseChangeAsync(int tenantId, int employeeId, int caseChangeId)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        if (await ParentService.GetParentIdAsync(Runtime.DbContext, employeeId) != tenantId)
        {
            return BadRequest($"Unknown tenant employee with id {employeeId}");
        }

        // employee check
        if (!await ParentService.ExistsAsync(Runtime.DbContext, employeeId))
        {
            return BadRequest($"Unknown employee with id {employeeId}");
        }

        return await GetAsync(employeeId, caseChangeId);
    }

    /// <summary>
    /// Query employee case changes values
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="employeeId">The employee id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The case change values array</returns>
    [HttpGet("values")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryEmployeeCaseChangesValues")]
    public async Task<ActionResult> QueryEmployeeCaseChangesValuesAsync(int tenantId, int employeeId,
        [FromQuery] DomainObject.CaseChangeQuery query) =>
        await QueryValuesAsync(tenantId, employeeId, query);

    /// <summary>
    /// Delete a case value
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="employeeId">The employee id</param>
    /// <param name="caseValueId">The case value id</param>
    [HttpDelete("{caseValueId}")]
    [ApiOperationId("DeleteEmployeeCaseChange")]
    public async Task<IActionResult> DeleteEmployeeCaseChangeAsync(int tenantId, int employeeId, int caseValueId)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await DeleteAsync(employeeId, caseValueId);
    }
}