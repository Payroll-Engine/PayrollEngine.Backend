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
[ApiControllerName("Shared regulations")]
[Route("api/shared/regulations/permissions")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.SharedRegulation)]
public abstract class SharedRegulationController : RepositoryRootObjectController<IRegulationPermissionService, IRegulationPermissionRepository,
    DomainObject.RegulationPermission, ApiObject.RegulationPermission>
{
    protected SharedRegulationController(IRegulationPermissionService permissionService, IControllerRuntime runtime) :
        base(permissionService, runtime, new RegulationPermissionMap())
    {
    }
}