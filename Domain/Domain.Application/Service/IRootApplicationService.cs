using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application.Service;

public interface IRootApplicationService<out TRepo, TDomain> : IRepositoryApplicationService<TRepo>
    where TRepo : class, IRootDomainRepository<TDomain>
    where TDomain : IDomainObject, new()
{
    Task<IEnumerable<TDomain>> QueryAsync(Query query = null);
    Task<long> QueryCountAsync(Query query);
    Task<TDomain> GetAsync(int itemId);
    Task<TDomain> CreateAsync(TDomain item);
    Task<IEnumerable<TDomain>> CreateAsync(IEnumerable<TDomain> items);
    Task<TDomain> UpdateAsync(TDomain item);
    Task<bool> DeleteAsync(int itemId);
}