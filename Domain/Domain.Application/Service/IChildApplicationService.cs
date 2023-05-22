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

    /// <summary>
    /// Test if item exists
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent item id</param>
    /// <param name="itemId">The item id</param>
    /// <returns>True if the item exists</returns>
    Task<bool> ExistsAsync(IDbContext context, int parentId, int itemId);

    /// <summary>
    /// Query items
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent item id</param>
    /// <param name="query">The item query</param>
    /// <returns>Item collection</returns>
    Task<IEnumerable<TDomain>> QueryAsync(IDbContext context, int parentId, Query query = null);


    /// <summary>
    /// Query items count
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent item id</param>
    /// <param name="query">The item query</param>
    /// <returns>Item count</returns>
    Task<long> QueryCountAsync(IDbContext context, int parentId, Query query = null);

    /// <summary>
    /// Get the parent id
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="itemId">The item id</param>
    Task<int?> GetParentIdAsync(IDbContext context, int itemId);

    /// <summary>
    /// Get item
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent item id</param>
    /// <param name="itemId">The item id</param>
    Task<TDomain> GetAsync(IDbContext context, int parentId, int itemId);

    /// <summary>
    /// Create new item
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent item id</param>
    /// <param name="item">The item</param>
    /// <returns>The new item</returns>
    Task<TDomain> CreateAsync(IDbContext context, int parentId, TDomain item);

    /// <summary>
    /// Create new items
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent item id</param>
    /// <param name="items">The items</param>
    /// <returns>The new items</returns>
    Task<IEnumerable<TDomain>> CreateAsync(IDbContext context, int parentId, IEnumerable<TDomain> items);

    /// <summary>
    /// Create bulk items
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent item id</param>
    /// <param name="items">The items</param>
    Task CreateBulkAsync(IDbContext context, int parentId, IEnumerable<TDomain> items);

    /// <summary>
    /// Update existing item
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent item id</param>
    /// <param name="item">The item</param>
    /// <returns>The updated item</returns>
    Task<TDomain> UpdateAsync(IDbContext context, int parentId, TDomain item);

    /// <summary>
    /// Delete existing item
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent item id</param>
    /// <param name="itemId">The item id</param>
    /// <returns>True for deleted item</returns>
    Task<bool> DeleteAsync(IDbContext context, int parentId, int itemId);
}