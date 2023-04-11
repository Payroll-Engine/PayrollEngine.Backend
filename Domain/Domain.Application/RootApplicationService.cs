using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public abstract class RootApplicationService<TRepo, TDomain> : RepositoryApplicationServiceBase<TRepo>, IRootApplicationService<TRepo, TDomain>
    where TRepo : class, IRootDomainRepository<TDomain>
    where TDomain : IDomainObject, new()
{
    protected RootApplicationService(TRepo repository) :
        base(repository)
    {
    }

    public virtual async Task<IEnumerable<TDomain>> QueryAsync(Query query = null) =>
        await Repository.QueryAsync(query);

    public virtual async Task<long> QueryCountAsync(Query query) =>
        await Repository.QueryCountAsync(query);

    public virtual async Task<TDomain> GetAsync(int itemId) =>
        await Repository.GetAsync(itemId);

    public virtual async Task<TDomain> CreateAsync(TDomain item) =>
        await Repository.CreateAsync(item);

    public virtual async Task<IEnumerable<TDomain>> CreateAsync(IEnumerable<TDomain> items) =>
        await Repository.CreateAsync(items);

    public virtual async Task<TDomain> UpdateAsync(TDomain item) =>
        await Repository.UpdateAsync(item);

    public virtual async Task<bool> DeleteAsync(int itemId) =>
        await Repository.DeleteAsync(itemId);
}