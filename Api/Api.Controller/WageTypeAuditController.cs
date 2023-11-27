using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the regulation wage type audits
/// </summary>
// ReSharper disable StringLiteralTypo
public abstract class WageTypeAuditController(IWageTypeService wageTypeService,
        IWageTypeAuditService wageTypeAuditService, IControllerRuntime runtime)
    : RepositoryChildObjectController<IWageTypeService, IWageTypeAuditService,
    IWageTypeRepository, IWageTypeAuditRepository,
    DomainObject.WageType, DomainObject.WageTypeAudit, ApiObject.WageTypeAudit>(wageTypeService, wageTypeAuditService, runtime, new WageTypeAuditMap());