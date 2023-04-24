using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Domain.Application.Service;

public interface IScriptTrackChildApplicationService<out TRepo, TDomain, TAudit> : IChildApplicationService<TRepo, TDomain>
    where TRepo : class, IScriptTrackDomainObjectRepository<TDomain, TAudit>, IChildDomainRepository<TDomain>
    where TDomain : TrackDomainObject<TAudit>, new()
    where TAudit : AuditDomainObject
{
    Task RebuildAsync(IDbContext context, int parentId, int itemId);
}