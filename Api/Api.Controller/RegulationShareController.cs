using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for shared regulations
/// </summary>
[ApiControllerName("Regulation shares")]
[Route("api/shares/regulations")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.RegulationShare)]
public abstract class RegulationShareController : RepositoryRootObjectController<IRegulationShareService, IRegulationShareRepository,
    DomainObject.RegulationShare, ApiObject.RegulationShare>
{
    protected RegulationShareController(IRegulationShareService shareService, IControllerRuntime runtime) :
        base(shareService, runtime, new RegulationShareMap())
    {
    }
}