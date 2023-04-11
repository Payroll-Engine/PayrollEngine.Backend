﻿using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the regulation case relation audits
/// </summary>
[ApiControllerName("Case relation audits")]
[Route("api/tenants/{tenantId}/regulations/{regulationId}/caserelations({relationId}/audits")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.CaseRelationAudit)]
public abstract class CaseRelationAuditController : RepositoryChildObjectController<ICaseRelationService, ICaseRelationAuditService,
    ICaseRelationRepository, ICaseRelationAuditRepository,
    DomainObject.CaseRelation, DomainObject.CaseRelationAudit, ApiObject.CaseRelationAudit>
{
    protected CaseRelationAuditController(ICaseRelationService caseRelationService,
        ICaseRelationAuditService caseRelationAuditService, IControllerRuntime runtime) :
        base(caseRelationService, caseRelationAuditService, runtime, new CaseRelationAuditMap())
    {
    }
}