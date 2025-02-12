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
/// API controller for the regulation wageTypes
/// </summary>
public abstract class WageTypeController(IRegulationService regulationService, IWageTypeService wageTypeService,
        IControllerRuntime runtime)
    : ScriptTrackChildObjectController<IRegulationService, IWageTypeService,
    IRegulationRepository, IWageTypeRepository,
    DomainObject.Regulation, DomainObject.WageType, DomainObject.WageTypeAudit, ApiObject.WageType>(regulationService, wageTypeService, runtime, new WageTypeMap())
{
    protected override async Task<ActionResult<ApiObject.WageType>> CreateAsync(int regulationId, ApiObject.WageType wageType)
    {
        // unique wage type name per payroll
        if (await ChildService.ExistsAnyAsync(Runtime.DbContext, regulationId, [wageType.WageTypeNumber]))
        {
            return BadRequest($"Wage type with number {wageType.WageTypeNumber} already exists");
        }

        return await base.CreateAsync(regulationId, wageType);
    }
}