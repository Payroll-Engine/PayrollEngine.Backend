﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Employees")]
[Route("api/tenants/{tenantId}/employees")]
public class EmployeeController : Api.Controller.EmployeeController
{
    /// <inheritdoc/>
    public EmployeeController(ITenantService tenantService, IEmployeeService employeeService,
        IControllerRuntime runtime) :
        base(tenantService, employeeService, runtime)
    {
    }

    /// <summary>
    /// Query employees
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>An ActionResult with employee array</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryEmployees")]
    public async Task<ActionResult> QueryEmployeesAsync(int tenantId, [FromQuery] DivisionQuery query = null)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await QueryItemsAsync(tenantId, query);
    }

    /// <summary>
    /// Get employee
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="employeeId">The id of the employee</param>
    [HttpGet("{employeeId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetEmployee")]
    public async Task<ActionResult<ApiObject.Employee>> GetEmployeeAsync(int tenantId, int employeeId)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await GetAsync(tenantId, employeeId);
    }

    /// <summary>
    /// Add a new employee
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="employee">The employee to add</param>
    /// <returns>The newly created employee</returns>
    [HttpPost]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("CreateEmployee")]
    public async Task<ActionResult<ApiObject.Employee>> CreateEmployeeAsync(int tenantId, ApiObject.Employee employee)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        // unique employee by identifier
        if (await Service.ExistsAnyAsync(Runtime.DbContext, tenantId, employee.Identifier))
        {
            return BadRequest($"Employee with identifier {employee.Identifier} already exists");
        }
        return await CreateAsync(tenantId, employee);
    }

    /// <summary>
    /// Update a employee
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="employee">The employee with updated values</param>
    /// <returns>The modified employee</returns>
    [HttpPut("{employeeId}")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("UpdateEmployee")]
    public async Task<ActionResult<ApiObject.Employee>> UpdateEmployeeAsync(int tenantId, ApiObject.Employee employee)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await UpdateAsync(tenantId, employee);
    }

    /// <summary>
    /// Delete a employee
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="employeeId">The id of the employee</param>
    [HttpDelete("{employeeId}")]
    [ApiOperationId("DeleteEmployee")]
    public async Task<IActionResult> DeleteEmployeeAsync(int tenantId, int employeeId)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await DeleteAsync(tenantId, employeeId);
    }

    #region Attributes

    /// <summary>
    /// Get an employee attribute
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="employeeId">The id of the employee</param>
    /// <param name="attributeName">The attribute name</param>
    /// <returns>The attribute value as JSON</returns>
    [HttpGet("{employeeId}/attributes/{attributeName}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetEmployeeAttribute")]
    public virtual async Task<ActionResult<string>> GetEmployeeAttributeAsync(int tenantId, int employeeId, string attributeName)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await GetAttributeAsync(employeeId, attributeName);
    }

    /// <summary>
    /// Set an employee attribute
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="employeeId">The id of the employee</param>
    /// <param name="attributeName">The attribute name</param>
    /// <param name="value">The attribute value as JSON</param>
    /// <returns>The current attribute value as JSON</returns>
    [HttpPost("{employeeId}/attributes/{attributeName}")]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("SetEmployeeAttribute")]
    public virtual async Task<ActionResult<string>> SetEmployeeAttributeAsync(int tenantId, int employeeId, string attributeName,
        [FromBody] string value)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await SetAttributeAsync(employeeId, attributeName, value);
    }

    /// <summary>
    /// Delete an employee attribute
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="employeeId">The id of the employee</param>
    /// <param name="attributeName">The attribute name</param>
    /// <returns>True if the attribute was deleted</returns>
    [HttpDelete("{employeeId}/attributes/{attributeName}")]
    [ApiOperationId("DeleteEmployeeAttribute")]
    public virtual async Task<ActionResult<bool>> DeleteEmployeeAttributeAsync(int tenantId, int employeeId, string attributeName)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await DeleteAttributeAsync(employeeId, attributeName);
    }

    #endregion

}