using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Domain.Application;

public abstract class ChildApplicationService<TRepo, TDomain> : RepositoryApplicationServiceBase<TRepo>, IChildApplicationService<TRepo, TDomain>
    where TRepo : class, IChildDomainRepository<TDomain>
    where TDomain : IDomainObject, new()
{
    protected ChildApplicationService(TRepo repository) :
        base(repository)
    {
    }

    public virtual async Task<IEnumerable<TDomain>> QueryAsync(int parentId, Query query = null) =>
        await Repository.QueryAsync(parentId, query);

    public virtual async Task<long> QueryCountAsync(int parentId, Query query = null) =>
        await Repository.QueryCountAsync(parentId, query);

    public virtual async Task<int?> GetParentIdAsync(int itemId) =>
        await Repository.GetParentIdAsync(itemId);

    public virtual async Task<TDomain> GetAsync(int parentId, int itemId) =>
        await Repository.GetAsync(parentId, itemId);

    public virtual async Task<TDomain> CreateAsync(int parentId, TDomain item) =>
        await Repository.CreateAsync(parentId, item);

    public virtual async Task CreateBulkAsync(int parentId, IEnumerable<TDomain> items) =>
        await Repository.CreateBulkAsync(parentId, items);

    public virtual async Task<IEnumerable<TDomain>> CreateAsync(int parentId, IEnumerable<TDomain> items) =>
        await Repository.CreateAsync(parentId, items);

    public virtual async Task<TDomain> UpdateAsync(int parentId, TDomain item) =>
        await Repository.UpdateAsync(parentId, item);

    public virtual async Task<bool> DeleteAsync(int parentId, int itemId) =>
        await Repository.DeleteAsync(parentId, itemId);

}