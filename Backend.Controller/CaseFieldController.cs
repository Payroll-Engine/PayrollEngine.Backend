using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;
// ReSharper disable UnusedParameter.Global

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Case fields")]
[Route("api/tenants/{tenantId}/regulations/{regulationId}/cases/{caseId}/fields")]
public class CaseFieldController : Api.Controller.CaseFieldController
{
    /// <inheritdoc/>
    public CaseFieldController(ICaseService caseService, ICaseFieldService caseFieldService,
        IControllerRuntime runtime) :
        base(caseService, caseFieldService, runtime)
    {
    }

    /// <summary>
    /// Query regulation case fields
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="caseId">The case id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The regulation case fields</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("QueryCaseFields")]
    public async Task<ActionResult> QueryCaseFieldsAsync(int tenantId, int regulationId, int caseId, [FromQuery] Query query)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await QueryItemsAsync(caseId, query);
    }

    /// <summary>
    /// Get a regulation case field
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="caseId">The case id</param>
    /// <param name="caseFieldId">The case field id</param>
    /// <returns>The regulation case</returns>
    [HttpGet("{fieldId}")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("GetCaseField")]
    public async Task<ActionResult<ApiObject.CaseField>> GetCaseFieldAsync(int tenantId, int regulationId, int caseId, int caseFieldId)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await GetAsync(caseId, caseFieldId);
    }

    /// <summary>
    /// Add a new regulation case field
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="caseId">The case id</param>
    /// <param name="caseField">The case field to add</param>
    /// <returns>The newly created regulation case field</returns>
    [HttpPost]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("CreateCaseField")]
    public async Task<ActionResult<ApiObject.CaseField>> CreateCaseFieldAsync(int tenantId, int caseId, ApiObject.CaseField caseField)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await CreateAsync(caseId, caseField);
    }

    /// <summary>
    /// Update a regulation case field
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="caseId">The case id</param>
    /// <param name="caseField">The case field with updated values</param>
    /// <returns>The modified regulation case field</returns>
    [HttpPut("{fieldId}")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("UpdateCaseField")]
    public async Task<ActionResult<ApiObject.CaseField>> UpdateCaseFieldAsync(int tenantId, int caseId, ApiObject.CaseField caseField)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await UpdateAsync(caseId, caseField);
    }

    /// <summary>
    /// Delete a regulation case field
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="caseId">The case id</param>
    /// <param name="caseFieldId">The case field id</param>
    [HttpDelete("{fieldId}")]
    [ApiOperationId("DeleteCaseField")]
    public async Task<IActionResult> DeleteCaseFieldAsync(int tenantId, int caseId, int caseFieldId)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await DeleteAsync(caseId, caseFieldId);
    }
}