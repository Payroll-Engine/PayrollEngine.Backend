using System.Collections.Generic;
using System.Threading.Tasks;

namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Generic domain object repository with support of the basic CRUD operations
/// </summary>
/// <typeparam name="T">The hosted domain child object</typeparam>
public interface IChildDomainRepository<T> : IDomainRepository
    where T : IDomainObject
{
    /// <summary>
    /// Query resources
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent object id</param>
    /// <param name="query">The query parameters</param>
    /// <returns>A list of the resources, matching the query</returns>
    Task<IEnumerable<T>> QueryAsync(IDbContext context, int parentId, Query query = null);

    /// <summary>
    /// Count query of resources
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent object id</param>
    /// <param name="query">The query parameters</param>
    /// <returns>Resource count matching the query</returns>
    Task<long> QueryCountAsync(IDbContext context, int parentId, Query query = null);

    /// <summary>
    /// Get id of the parent object
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="itemId">The domain object id</param>
    /// <returns>The id of the parent object</returns>
    Task<int?> GetParentIdAsync(IDbContext context, int itemId);

    /// <summary>
    /// Get object by parent and id
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent object id</param>
    /// <param name="itemId">The domain object id</param>
    /// <returns>The id of the parent object</returns>
    Task<T> GetAsync(IDbContext context, int parentId, int itemId);

    /// <summary>
    /// Add a new domain object to the repository
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent object id</param>
    /// <param name="item">The domain object to add</param>
    /// <returns>The newly created domain object including the new id</returns>
    Task<T> CreateAsync(IDbContext context, int parentId, T item);

    /// <summary>
    /// Add multiple new domain objects to the repository
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent object id</param>
    /// <param name="items">The domain objects to add</param>
    /// <returns>The newly created domain objects including the new id</returns>
    Task<IEnumerable<T>> CreateAsync(IDbContext context, int parentId, IEnumerable<T> items);

    /// <summary>
    /// Add multiple new domain objects to the repository using bulk insert
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent object id</param>
    /// <param name="items">The domain objects to add</param>
    System.Threading.Tasks.Task CreateBulkAsync(IDbContext context, int parentId, IEnumerable<T> items);

    /// <summary>
    /// Update a repository domain object including his parent
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent object id</param>
    /// <param name="item">The domain object to update</param>
    /// <returns>The updated domain object</returns>
    Task<T> UpdateAsync(IDbContext context, int parentId, T item);

    /// <summary>
    /// Remove a domain object from the repository
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="parentId">The parent object id</param>
    /// <param name="itemId">The if of the domain object to delete</param>
    /// <returns>True if the record was deleted</returns>
    Task<bool> DeleteAsync(IDbContext context, int parentId, int itemId);
}