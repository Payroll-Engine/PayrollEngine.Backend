using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the regulation collector audits
/// </summary>
public abstract class CollectorAuditController(ICollectorService collectorService,
        ICollectorAuditService collectorAuditService, IControllerRuntime runtime)
    : RepositoryChildObjectController<ICollectorService, ICollectorAuditService, 
    ICollectorRepository, ICollectorAuditRepository,
    DomainObject.Collector, DomainObject.CollectorAudit, ApiObject.CollectorAudit>(collectorService, collectorAuditService, runtime, new CollectorAuditMap());