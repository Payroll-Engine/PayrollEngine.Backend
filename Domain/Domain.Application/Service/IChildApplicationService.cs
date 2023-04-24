using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Domain.Application.Service;

public interface IChildApplicationService<out TRepo, TDomain> : IRepositoryApplicationService<TRepo>
    where TRepo : class, IChildDomainRepository<TDomain>
    where TDomain : IDomainObject, new()
{
    Task<IEnumerable<TDomain>> QueryAsync(IDbContext context, int parentId, Query query = null);
    Task<long> QueryCountAsync(IDbContext context, int parentId, Query query = null);
    Task<int?> GetParentIdAsync(IDbContext context, int itemId);
    Task<TDomain> GetAsync(IDbContext context, int parentId, int itemId);
    Task<TDomain> CreateAsync(IDbContext context, int parentId, TDomain item);
    Task<IEnumerable<TDomain>> CreateAsync(IDbContext context, int parentId, IEnumerable<TDomain> items);
    Task CreateBulkAsync(IDbContext context, int parentId, IEnumerable<TDomain> items);
    Task<TDomain> UpdateAsync(IDbContext context, int parentId, TDomain item);
    Task<bool> DeleteAsync(IDbContext context, int parentId, int itemId);
}