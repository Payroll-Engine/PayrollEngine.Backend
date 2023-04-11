using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the regulation cases
/// </summary>
[ApiControllerName("Cases")]
[Route("api/tenants/{tenantId}/regulations/{regulationId}/cases")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.Case)]
public abstract class CaseController : ScriptTrackChildObjectController<IRegulationService, ICaseService,
    IRegulationRepository, ICaseRepository,
    DomainObject.Regulation, DomainObject.Case, DomainObject.CaseAudit, ApiObject.Case>
{
    protected CaseController(IRegulationService regulationService, ICaseService caseService, IControllerRuntime runtime) :
        base(regulationService, caseService, runtime, new CaseMap())
    {
    }
}