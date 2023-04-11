﻿using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the regulation collector audits
/// </summary>
[ApiControllerName("Collector audits")]
[Route("api/tenants/{tenantId}/regulations/{regulationId}/collectors/{collectorId}/audits")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.CollectorAudit)]
public abstract class CollectorAuditController : RepositoryChildObjectController<ICollectorService, ICollectorAuditService, 
    ICollectorRepository, ICollectorAuditRepository,
    DomainObject.Collector, DomainObject.CollectorAudit, ApiObject.CollectorAudit>
{
    protected CollectorAuditController(ICollectorService collectorService, ICollectorAuditService collectorAuditService, IControllerRuntime runtime) :
        base(collectorService, collectorAuditService, runtime, new CollectorAuditMap())
    {
    }
}