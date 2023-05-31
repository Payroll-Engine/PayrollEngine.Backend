using System.Threading.Tasks;
using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the regulation collectors
/// </summary>
public abstract class CollectorController : ScriptTrackChildObjectController<IRegulationService, ICollectorService,
    IRegulationRepository, ICollectorRepository,
    DomainObject.Regulation, DomainObject.Collector, DomainObject.CollectorAudit, ApiObject.Collector>
{
    protected CollectorController(IRegulationService regulationService, ICollectorService collectorService, IControllerRuntime runtime) :
        base(regulationService, collectorService, runtime, new CollectorMap())
    {
    }

    protected override async Task<ActionResult<ApiObject.Collector>> CreateAsync(int regulationId, ApiObject.Collector collector)
    {
        if (string.IsNullOrWhiteSpace(collector.Name))
        {
            return BadRequest($"Collector {collector.Id} without name");
        }
        // unique collector name per payroll regulation
        if (await ChildService.ExistsAnyAsync(Runtime.DbContext, regulationId, new[] { collector.Name }))
        {
            return BadRequest($"Collector with name {collector.Name} already exists");
        }

        return await base.CreateAsync(regulationId, collector);
    }
}