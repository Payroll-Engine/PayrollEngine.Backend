using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Case relations")]
[Route("api/tenants/{tenantId}/regulations/{regulationId}/caserelations")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.CaseRelation)]
public class CaseRelationController : Api.Controller.CaseRelationController
{
    /// <inheritdoc/>
    public CaseRelationController(IRegulationService regulationService, ICaseService caseService,
        ICaseRelationService caseRelationService, IControllerRuntime runtime) :
        base(regulationService, caseService, caseRelationService, runtime)
    {
    }

    /// <summary>
    /// Query regulation case relations
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The regulation case fields</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryCaseRelations")]
    public async Task<ActionResult> QueryCaseRelationsAsync(int tenantId, int regulationId, [FromQuery] Query query)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await QueryItemsAsync(regulationId, query);
    }

    /// <summary>
    /// Get a regulation case relation
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="relationId">The case id</param>
    /// <returns>The regulation case</returns>
    [HttpGet("{relationId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetCaseRelation")]
    public async Task<ActionResult<ApiObject.CaseRelation>> GetCaseRelationAsync(int tenantId, int regulationId, int relationId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await GetAsync(regulationId, relationId);
    }

    /// <summary>
    /// Add a new regulation case relation
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="caseRelation">The case relation to add</param>
    /// <returns>The newly created regulation case relation</returns>
    [HttpPost]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("CreateCaseRelation")]
    public async Task<ActionResult<ApiObject.CaseRelation>> CreateCaseRelationAsync(int tenantId,
        int regulationId, ApiObject.CaseRelation caseRelation)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await CreateAsync(regulationId, caseRelation);
    }

    /// <summary>
    /// Update a regulation case relation
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="caseRelation">The case relation with updated values</param>
    /// <returns>The modified regulation case relation</returns>
    [HttpPut("{relationId}")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("UpdateCaseRelation")]
    public async Task<ActionResult<ApiObject.CaseRelation>> UpdateCaseRelationAsync(int tenantId,
        int regulationId, ApiObject.CaseRelation caseRelation)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await UpdateAsync(regulationId, caseRelation);
    }

    /// <summary>
    /// Rebuild regulation case relation
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="relationId">The id of the case relation</param>
    [HttpPut("{relationId}/rebuild")]
    [NotFoundResponse]
    [ApiOperationId("RebuildCaseRelation")]
    public async Task<ActionResult> RebuildCaseRelationAsync(int tenantId, int regulationId, int relationId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await RebuildAsync(regulationId, relationId);
    }

    /// <summary>
    /// Delete a regulation case relation
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="relationId">The id of the case relation</param>
    [HttpDelete("{relationId}")]
    [ApiOperationId("DeleteCaseRelation")]
    public async Task<IActionResult> DeleteCaseRelationAsync(int tenantId, int regulationId, int relationId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await DeleteAsync(regulationId, relationId);
    }
}