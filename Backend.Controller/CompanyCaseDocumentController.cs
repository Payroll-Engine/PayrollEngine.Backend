﻿using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Company case documents")]
[Route("api/tenants/{tenantId}/companycases/{caseValueId}/documents")]
public class CompanyCaseDocumentController : Api.Controller.CompanyCaseDocumentController
{
    /// <inheritdoc/>
    public CompanyCaseDocumentController(ICompanyCaseValueService caseValueService, ICompanyCaseDocumentService caseDocumentService,
        IControllerRuntime runtime) :
        base(caseValueService, caseDocumentService, runtime)
    {
    }

    /// <summary>
    /// Query company case documents
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="caseValueId">The case value id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The company case documents</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryCompanyCaseDocuments")]
    public async Task<ActionResult> QueryCompanyCaseDocumentsAsync(int tenantId, int caseValueId, [FromQuery] Query query)
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
    /// Get a company case document
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="caseValueId">The case value id</param>
    /// <param name="documentId">The document id</param>
    /// <returns>The company case document</returns>
    [HttpGet("{documentId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetCompanyCaseDocument")]
    public async Task<ActionResult<ApiObject.CaseDocument>> GetCompanyCaseDocument(int tenantId, int caseValueId, int documentId)
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