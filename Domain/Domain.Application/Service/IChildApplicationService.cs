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
    Task<IEnumerable<TDomain>> QueryAsync(int parentId, Query query = null);
    Task<long> QueryCountAsync(int parentId, Query query = null);
    Task<int?> GetParentIdAsync(int itemId);
    Task<TDomain> GetAsync(int parentId, int itemId);
    Task<TDomain> CreateAsync(int parentId, TDomain item);
    Task<IEnumerable<TDomain>> CreateAsync(int parentId, IEnumerable<TDomain> items);
    Task CreateBulkAsync(int parentId, IEnumerable<TDomain> items);
    Task<TDomain> UpdateAsync(int parentId, TDomain item);
    Task<bool> DeleteAsync(int parentId, int itemId);
}