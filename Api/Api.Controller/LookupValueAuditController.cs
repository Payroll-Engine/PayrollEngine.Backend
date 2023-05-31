using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the regulation lookup value audits
/// </summary>
public abstract class LookupValueAuditController : RepositoryChildObjectController<ILookupValueService, ILookupValueAuditService,
    ILookupValueRepository, ILookupValueAuditRepository,
    DomainObject.LookupValue, DomainObject.LookupValueAudit, ApiObject.LookupValueAudit>
{
    protected LookupValueAuditController(ILookupValueService lookupValueService, ILookupValueAuditService caseFieldAuditService, IControllerRuntime runtime) :
        base(lookupValueService, caseFieldAuditService, runtime, new LookupValueAuditMap())
    {
    }
}