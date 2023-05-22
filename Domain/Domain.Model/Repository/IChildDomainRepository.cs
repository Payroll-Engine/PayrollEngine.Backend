using System.Collections.Generic;
using System.Threading.Tasks;

namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Generic domain item repository with support of the basic CRUD operations
/// </summary>
/// <typeparam name="T">The hosted domain child item</typeparam>
public interface IChildDomainRepository<T> : IDomainRepository
    where T : IDomainObject
{
    /// <summary>
    /// Test if an domain item exists
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent item id</param>
    /// <param name="id">The item id</param>
    /// <returns>True, if the domain item exists</returns>
    Task<bool> ExistsAsync(IDbContext context, int parentId, int id);

    /// <summary>
    /// Query resources
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent item id</param>
    /// <param name="query">The query parameters</param>
    /// <returns>A list of the resources, matching the query</returns>
    Task<IEnumerable<T>> QueryAsync(IDbContext context, int parentId, Query query = null);

    /// <summary>
    /// Count query of resources
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent item id</param>
    /// <param name="query">The query parameters</param>
    /// <returns>Resource count matching the query</returns>
    Task<long> QueryCountAsync(IDbContext context, int parentId, Query query = null);

    /// <summary>
    /// Get id of the parent item
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="itemId">The domain item id</param>
    /// <returns>The id of the parent item</returns>
    Task<int?> GetParentIdAsync(IDbContext context, int itemId);

    /// <summary>
    /// Get item by parent and id
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent item id</param>
    /// <param name="itemId">The domain item id</param>
    /// <returns>The id of the parent item</returns>
    Task<T> GetAsync(IDbContext context, int parentId, int itemId);

    /// <summary>
    /// Add a new domain item to the repository
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent item id</param>
    /// <param name="item">The domain item to add</param>
    /// <returns>The newly created domain item including the new id</returns>
    Task<T> CreateAsync(IDbContext context, int parentId, T item);

    /// <summary>
    /// Add multiple new domain items to the repository
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent item id</param>
    /// <param name="items">The domain items to add</param>
    /// <returns>The newly created domain items including the new id</returns>
    Task<IEnumerable<T>> CreateAsync(IDbContext context, int parentId, IEnumerable<T> items);

    /// <summary>
    /// Add multiple new domain items to the repository using bulk insert
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent item id</param>
    /// <param name="items">The domain items to add</param>
    System.Threading.Tasks.Task CreateBulkAsync(IDbContext context, int parentId, IEnumerable<T> items);

    /// <summary>
    /// Update a repository domain item including his parent
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent item id</param>
    /// <param name="item">The domain item to update</param>
    /// <returns>The updated domain item</returns>
    Task<T> UpdateAsync(IDbContext context, int parentId, T item);

    /// <summary>
    /// Remove a domain item from the repository
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent item id</param>
    /// <param name="itemId">The if of the domain item to delete</param>
    /// <returns>True if the record was deleted</returns>
    Task<bool> DeleteAsync(IDbContext context, int parentId, int itemId);
}