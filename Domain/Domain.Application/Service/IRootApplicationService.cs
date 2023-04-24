using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application.Service;

public interface IRootApplicationService<out TRepo, TDomain> : IRepositoryApplicationService<TRepo>
    where TRepo : class, IRootDomainRepository<TDomain>
    where TDomain : IDomainObject, new()
{
    Task<IEnumerable<TDomain>> QueryAsync(IDbContext context, Query query = null);
    Task<long> QueryCountAsync(IDbContext context, Query query);
    Task<TDomain> GetAsync(IDbContext context, int itemId);
    Task<TDomain> CreateAsync(IDbContext context, TDomain item);
    Task<IEnumerable<TDomain>> CreateAsync(IDbContext context, IEnumerable<TDomain> items);
    Task<TDomain> UpdateAsync(IDbContext context, TDomain item);
    Task<bool> DeleteAsync(IDbContext context, int itemId);
}