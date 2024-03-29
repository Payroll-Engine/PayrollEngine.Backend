﻿using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Global case documents")]
[Route("api/tenants/{tenantId}/globalcases/{caseValueId}/documents")]
public class GlobalCaseDocumentController : Api.Controller.GlobalCaseDocumentController
{
    /// <inheritdoc/>
    public GlobalCaseDocumentController(IGlobalCaseValueService caseValueService, IGlobalCaseDocumentService caseDocumentService,
        IControllerRuntime runtime) :
        base(caseValueService, caseDocumentService, runtime)
    {
    }

    /// <summary>
    /// Query global case documents
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="caseValueId">The case value id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The global case documents</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryGlobalCaseDocuments")]
    public async Task<ActionResult> QueryGlobalCaseDocumentsAsync(int tenantId, 
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
    /// Get a global case document
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="caseValueId">The case value id</param>
    /// <param name="documentId">The document id</param>
    /// <returns>The global case document</returns>
    [HttpGet("{documentId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetGlobalCaseDocument")]
    public async Task<ActionResult<ApiObject.CaseDocument>> GetGlobalCaseDocumentAsync(
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