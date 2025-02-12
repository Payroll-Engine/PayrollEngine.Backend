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
/// API controller for the regulation case fields
/// </summary>
public abstract class CaseFieldController(ICaseService caseService, ICaseFieldService caseFieldService,
        IControllerRuntime runtime)
    : RepositoryChildObjectController<ICaseService, ICaseFieldService,
    ICaseRepository, ICaseFieldRepository,
    DomainObject.Case, DomainObject.CaseField, ApiObject.CaseField>(caseService, caseFieldService, runtime, new CaseFieldMap())
{
    protected override async Task<ActionResult<ApiObject.CaseField>> CreateAsync(int regulationId, ApiObject.CaseField caseField)
    {
        // unique case field name per case
        if (await ChildService.ExistsAnyAsync(Runtime.DbContext, regulationId, [caseField.Name]))
        {
            return BadRequest($"Case filed with name {caseField.Name} already exists");
        }

        return await base.CreateAsync(regulationId, caseField);
    }
}