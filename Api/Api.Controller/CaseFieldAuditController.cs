using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the regulation case field audits
/// </summary>
public abstract class CaseFieldAuditController(ICaseFieldService caseFieldService,
        ICaseFieldAuditService caseFieldAuditService, IControllerRuntime runtime)
    : RepositoryChildObjectController<ICaseFieldService, ICaseFieldAuditService,
    ICaseFieldRepository, ICaseFieldAuditRepository,
    DomainObject.CaseField, DomainObject.CaseFieldAudit, ApiObject.CaseFieldAudit>(caseFieldService, caseFieldAuditService, runtime, new CaseFieldAuditMap());