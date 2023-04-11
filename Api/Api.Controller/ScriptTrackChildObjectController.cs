using System.Threading.Tasks;
using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Model.Repository;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the script track objects
/// </summary>
public abstract class ScriptTrackChildObjectController<TParentService, TService, TParentRepo, TRepo, TParent, TDomain, TAudit, TApi> :
    RepositoryChildObjectController<TParentService, TService, TParentRepo, TRepo, TParent, TDomain, TApi>
    where TParentService : class, IRepositoryApplicationService<TParentRepo>
    where TService : class, IScriptTrackChildApplicationService<TRepo, TDomain, TAudit>, IChildApplicationService<TRepo, TDomain>
    where TParentRepo : class, IDomainRepository
    where TRepo : class, IScriptTrackDomainObjectRepository<TDomain, TAudit>, IChildDomainRepository<TDomain>
    where TParent : class, DomainObject.IDomainObject, new()
    where TDomain : DomainObject.TrackDomainObject<TAudit>, new()
    where TAudit : DomainObject.AuditDomainObject
    where TApi : ApiObject.ApiObjectBase, new()
{
    protected ScriptTrackChildObjectController(TParentService parentService, TService service, IControllerRuntime runtime,
        IApiMap<TDomain, TApi> map) :
        base(parentService, service, runtime, map)
    {
    }

    // duplicated in PayrunController!
    protected virtual async Task<ActionResult> RebuildAsync(int parentId, int itemId)
    {
        if (parentId <= 0)
        {
            return InvalidParentRequest(parentId);
        }

        // test item
        if (!await ChildService.ExistsAsync(itemId))
        {
            return BadRequest($"Unknown script object with id {itemId}");
        }

        await Service.RebuildAsync(parentId, itemId);
        return Ok();
    }
}