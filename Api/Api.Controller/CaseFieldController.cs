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
[ApiControllerName("Case fields")]
[Route("api/tenants/{tenantId}/regulations/{regulationId}/cases/{caseId}/fields")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.CaseField)]
public abstract class CaseFieldController : RepositoryChildObjectController<ICaseService, ICaseFieldService,
    ICaseRepository, ICaseFieldRepository,
    DomainObject.Case, DomainObject.CaseField, ApiObject.CaseField>
{
    protected CaseFieldController(ICaseService caseService, ICaseFieldService caseFieldService, IControllerRuntime runtime) :
        base(caseService, caseFieldService, runtime, new CaseFieldMap())
    {
    }

    protected override async Task<ActionResult<ApiObject.CaseField>> CreateAsync(int regulationId, ApiObject.CaseField caseField)
    {
        // unique case field name per case
        if (await ChildService.ExistsAnyAsync(regulationId, new[] { caseField.Name }))
        {
            return BadRequest($"Case filed with name {caseField.Name} already exists");
        }

        return await base.CreateAsync(regulationId, caseField);
    }
}