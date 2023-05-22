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
    
    public virtual async Task<bool> ExistsAsync(IDbContext context, int parentId, int itemId) =>
        await Repository.ExistsAsync(context, parentId, itemId);

    public virtual async Task<IEnumerable<TDomain>> QueryAsync(IDbContext context, int parentId, Query query = null) =>
        await Repository.QueryAsync(context, parentId, query);

    public virtual async Task<long> QueryCountAsync(IDbContext context, int parentId, Query query = null) =>
        await Repository.QueryCountAsync(context, parentId, query);

    public virtual async Task<int?> GetParentIdAsync(IDbContext context, int itemId) =>
        await Repository.GetParentIdAsync(context, itemId);

    public virtual async Task<TDomain> GetAsync(IDbContext context, int parentId, int itemId) =>
        await Repository.GetAsync(context, parentId, itemId);

    public virtual async Task<TDomain> CreateAsync(IDbContext context, int parentId, TDomain item) =>
        await Repository.CreateAsync(context, parentId, item);

    public virtual async Task CreateBulkAsync(IDbContext context, int parentId, IEnumerable<TDomain> items) =>
        await Repository.CreateBulkAsync(context, parentId, items);

    public virtual async Task<IEnumerable<TDomain>> CreateAsync(IDbContext context, int parentId, IEnumerable<TDomain> items) =>
        await Repository.CreateAsync(context, parentId, items);

    public virtual async Task<TDomain> UpdateAsync(IDbContext context, int parentId, TDomain item) =>
        await Repository.UpdateAsync(context, parentId, item);

    public virtual async Task<bool> DeleteAsync(IDbContext context, int parentId, int itemId) =>
        await Repository.DeleteAsync(context, parentId, itemId);

}