using System.Threading.Tasks;
using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Model.Repository;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using PayrollEngine.Domain.Model;

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
    where TParent : class, IDomainObject, new()
    where TDomain : TrackDomainObject<TAudit>, new()
    where TAudit : AuditDomainObject
    where TApi : ApiObject.ApiObjectBase, new()
{
    protected ScriptTrackChildObjectController(TParentService parentService, TService service, IControllerRuntime runtime,
        IApiMap<TDomain, TApi> map) :
        base(parentService, service, runtime, map)
    {
    }

    // duplicated in PayrunController!
    protected async Task<ActionResult> RebuildAsync(int parentId, int itemId)
    {
        if (parentId <= 0)
        {
            return InvalidParentRequest(parentId);
        }

        // test item
        if (!await ChildService.ExistsAsync(Runtime.DbContext, itemId))
        {
            return BadRequest($"Unknown script object with id {itemId}");
        }

        await Service.RebuildAsync(Runtime.DbContext, parentId, itemId);
        return Ok();
    }
}