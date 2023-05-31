using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the regulation cases
/// </summary>
public abstract class CaseController : ScriptTrackChildObjectController<IRegulationService, ICaseService,
    IRegulationRepository, ICaseRepository,
    DomainObject.Regulation, DomainObject.Case, DomainObject.CaseAudit, ApiObject.Case>
{
    protected CaseController(IRegulationService regulationService, ICaseService caseService, IControllerRuntime runtime) :
        base(regulationService, caseService, runtime, new CaseMap())
    {
    }
}