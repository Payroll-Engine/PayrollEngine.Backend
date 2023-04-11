using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the regulation wage type audits
/// </summary>
// ReSharper disable StringLiteralTypo
[ApiControllerName("Wage type audits")]
[Route("api/tenants/{tenantId}/regulations/{regulationId}/wagetypes/{wageTypeId}/audits")]
// ReSharper restore StringLiteralTypo
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.WageTypeAudit)]
public abstract class WageTypeAuditController : RepositoryChildObjectController<IWageTypeService, IWageTypeAuditService,
    IWageTypeRepository, IWageTypeAuditRepository,
    DomainObject.WageType, DomainObject.WageTypeAudit, ApiObject.WageTypeAudit>
{
    protected WageTypeAuditController(IWageTypeService wageTypeService, IWageTypeAuditService wageTypeAuditService, IControllerRuntime runtime) :
        base(wageTypeService, wageTypeAuditService, runtime, new WageTypeAuditMap())
    {
    }
}