using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the regulation case field audits
/// </summary>
[ApiControllerName("Case field audits")]
[Route("api/tenants/{tenantId}/regulations/{regulationId}/cases/{caseId}/fields/{fieldId}/audits")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.CaseFieldAudit)]
public abstract class CaseFieldAuditController : RepositoryChildObjectController<ICaseFieldService, ICaseFieldAuditService,
    ICaseFieldRepository, ICaseFieldAuditRepository,
    DomainObject.CaseField, DomainObject.CaseFieldAudit, ApiObject.CaseFieldAudit>
{
    protected CaseFieldAuditController(ICaseFieldService caseFieldService, ICaseFieldAuditService caseFieldAuditService, IControllerRuntime runtime) :
        base(caseFieldService, caseFieldAuditService, runtime, new CaseFieldAuditMap())
    {
    }
}