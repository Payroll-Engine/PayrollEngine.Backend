using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public abstract class RootApplicationService<TRepo, TDomain>
    (TRepo repository) : RepositoryApplicationServiceBase<TRepo>(repository), IRootApplicationService<TRepo, TDomain>
    where TRepo : class, IRootDomainRepository<TDomain>
    where TDomain : IDomainObject, new()
{
    public virtual async Task<IEnumerable<TDomain>> QueryAsync(IDbContext context, Query query = null) =>
        await Repository.QueryAsync(context, query);

    public virtual async Task<long> QueryCountAsync(IDbContext context, Query query) =>
        await Repository.QueryCountAsync(context, query);

    public virtual async Task<TDomain> GetAsync(IDbContext context, int itemId) =>
        await Repository.GetAsync(context, itemId);

    public virtual async Task<TDomain> CreateAsync(IDbContext context, TDomain item) =>
        await Repository.CreateAsync(context, item);

    public virtual async Task<IEnumerable<TDomain>> CreateAsync(IDbContext context, IEnumerable<TDomain> items) =>
        await Repository.CreateAsync(context, items);

    public virtual async Task<TDomain> UpdateAsync(IDbContext context, TDomain item) =>
        await Repository.UpdateAsync(context, item);

    public virtual async Task<bool> DeleteAsync(IDbContext context, int itemId) =>
        await Repository.DeleteAsync(context, itemId);
}