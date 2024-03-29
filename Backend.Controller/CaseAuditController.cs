﻿using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;
// ReSharper disable UnusedParameter.Global

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Case audits")]
[Route("api/tenants/{tenantId}/regulations/{regulationId}/cases/{caseId}/audits")]
public class CaseAuditController : Api.Controller.CaseAuditController
{
    /// <inheritdoc/>
    public CaseAuditController(ICaseService caseService,
        ICaseAuditService caseAuditService, IControllerRuntime runtime) :
        base(caseService, caseAuditService, runtime)
    {
    }

    /// <summary>
    /// Query regulation case audits
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="caseId">The case id </param>
    /// <param name="regulationId">The regulation id </param>
    /// <param name="query">Query parameters</param>
    /// <returns>The audit objects</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("QueryCaseAudits")]
    public async Task<ActionResult> QueryCaseAuditsAsync(int tenantId, int regulationId, int caseId, [FromQuery] Query query)
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
    /// Get a regulation case audit
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id </param>
    /// <param name="caseId">The case id</param>
    /// <param name="auditId">The audit object id</param>
    /// <returns>The audit object</returns>
    [HttpGet("{auditId}")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("GetCaseAudit")]
    public async Task<ActionResult<ApiObject.CaseAudit>> GetCaseAuditAsync(int tenantId, int regulationId, int caseId, int auditId)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await GetAsync(caseId, auditId);
    }
}