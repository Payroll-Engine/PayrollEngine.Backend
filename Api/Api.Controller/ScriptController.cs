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
/// API controller for the regulation scripts
/// </summary>
public abstract class ScriptController : RepositoryChildObjectController<IRegulationService, IScriptService,
    IRegulationRepository, IScriptRepository,
    DomainObject.Regulation, DomainObject.Script, ApiObject.Script>
{
    protected ScriptController(IRegulationService regulationService, IScriptService scriptService, IControllerRuntime runtime) :
        base(regulationService, scriptService, runtime, new ScriptMap())
    {
    }

    protected override async Task<ActionResult<ApiObject.Script>> CreateAsync(int regulationId, ApiObject.Script script)
    {
        if (string.IsNullOrWhiteSpace(script.Name))
        {
            return BadRequest($"Script {script.Id} without name");
        }
        // unique function name per payroll
        if (await ChildService.ExistsAnyAsync(Runtime.DbContext, regulationId, new[] { script.Name }))
        {
            return BadRequest($"Script with name {script.Name} already exists");
        }

        return await base.CreateAsync(regulationId, script);
    }
}