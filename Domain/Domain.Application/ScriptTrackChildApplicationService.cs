using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Domain.Application;

public abstract class ScriptTrackChildApplicationService<TRepo, TDomain, TAudit>(TRepo repository) :
    ChildApplicationService<TRepo, TDomain>(repository),
    IScriptTrackChildApplicationService<TRepo, TDomain, TAudit>
    where TRepo : class, IScriptTrackDomainObjectRepository<TDomain, TAudit>, IChildDomainRepository<TDomain>
    where TDomain : TrackDomainObject<TAudit>, new()
    where TAudit : AuditDomainObject
{
    public virtual async Task RebuildAsync(IDbContext context, int parentId, int itemId) =>
        await Repository.RebuildAsync(context, parentId, itemId);
}