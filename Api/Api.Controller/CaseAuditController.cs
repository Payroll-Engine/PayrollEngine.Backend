﻿using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the regulation case audits
/// </summary>
public abstract class CaseAuditController : RepositoryChildObjectController<ICaseService, ICaseAuditService, 
    ICaseRepository, ICaseAuditRepository,
    DomainObject.Case, DomainObject.CaseAudit, ApiObject.CaseAudit>
{
    protected CaseAuditController(ICaseService caseService, ICaseAuditService caseFieldAuditService, IControllerRuntime runtime) :
        base(caseService, caseFieldAuditService, runtime, new CaseAuditMap())
    {
    }
}