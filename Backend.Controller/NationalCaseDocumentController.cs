using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("National case documents")]
[Route("api/tenants/{tenantId}/nationalcases/{caseValueId}/documents")]
public class NationalCaseDocumentController : Api.Controller.NationalCaseDocumentController
{
    /// <inheritdoc/>
    public NationalCaseDocumentController(INationalCaseValueService caseValueService,
        INationalCaseDocumentService caseDocumentService, IControllerRuntime runtime) :
        base(caseValueService, caseDocumentService, runtime)
    {
    }

    /// <summary>
    /// Query national case documents
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="caseValueId">The case value id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The national case documents</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryNationalCaseDocuments")]
    public async Task<ActionResult> QueryNationalCaseDocumentsAsync(int tenantId,
        int caseValueId, [FromQuery] Query query)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await QueryItemsAsync(caseValueId, query);
    }

    /// <summary>
    /// Get a national case document
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="caseValueId">The case value id</param>
    /// <param name="documentId">The document id</param>
    /// <returns>The national case document</returns>
    [HttpGet("{documentId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetNationalCaseDocument")]
    public async Task<ActionResult<ApiObject.CaseDocument>> GetNationalCaseDocumentAsync(
        int tenantId, int caseValueId, int documentId)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await GetAsync(caseValueId, documentId);
    }
}