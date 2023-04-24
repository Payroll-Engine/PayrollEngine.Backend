using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Domain.Application;

public abstract class ScriptTrackChildApplicationService<TRepo, TDomain, TAudit> : ChildApplicationService<TRepo, TDomain>,
    IScriptTrackChildApplicationService<TRepo, TDomain, TAudit>
    where TRepo : class, IScriptTrackDomainObjectRepository<TDomain, TAudit>, IChildDomainRepository<TDomain>
    where TDomain : TrackDomainObject<TAudit>, new()
    where TAudit : AuditDomainObject
{
    protected ScriptTrackChildApplicationService(TRepo repository) :
        base(repository)
    {
    }

    public virtual async Task RebuildAsync(IDbContext context, int parentId, int itemId) =>
        await Repository.RebuildAsync(context, parentId, itemId);
}