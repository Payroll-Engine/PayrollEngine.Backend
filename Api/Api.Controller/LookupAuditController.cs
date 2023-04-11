using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the regulation lookup audits
/// </summary>
[ApiControllerName("Lookup audits")]
[Route("api/tenants/{tenantId}/regulations/{regulationId}/lookups/{lookupId}/audits")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.LookupAudit)]
public abstract class LookupAuditController : RepositoryChildObjectController<ILookupService, ILookupAuditService,
    ILookupRepository, ILookupAuditRepository,
    DomainObject.Lookup, DomainObject.LookupAudit, ApiObject.LookupAudit>
{
    protected LookupAuditController(ILookupService lookupService, ILookupAuditService caseFieldAuditService, IControllerRuntime runtime) :
        base(lookupService, caseFieldAuditService, runtime, new LookupAuditMap())
    {
    }
}