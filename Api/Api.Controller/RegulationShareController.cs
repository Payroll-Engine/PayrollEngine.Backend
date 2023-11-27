using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for shared regulations
/// </summary>
public abstract class RegulationShareController(IRegulationShareService shareService, IControllerRuntime runtime)
    : RepositoryRootObjectController<IRegulationShareService, IRegulationShareRepository,
    DomainObject.RegulationShare, ApiObject.RegulationShare>(shareService, runtime, new RegulationShareMap());