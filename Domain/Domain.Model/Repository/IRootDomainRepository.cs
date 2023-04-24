using System.Collections.Generic;
using System.Threading.Tasks;

namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Generic domain object repository with support of the basic CRUD operations
/// </summary>
/// <typeparam name="T">The hosted root domain object</typeparam>
public interface IRootDomainRepository<T> : IDomainRepository
    where T : IDomainObject
{
    /// <summary>
    /// Query resources
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The query parameters</param>
    /// <returns>A list of the resources, matching the query</returns>
    Task<IEnumerable<T>> QueryAsync(IDbContext context, Query query = null);

    /// <summary>
    /// Count query of resources
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="query">The query parameters</param>
    /// <returns>Resource count matching the query</returns>
    Task<long> QueryCountAsync(IDbContext context, Query query = null);

    /// <summary>
    /// Get one specific domain object by id
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="id">The object id</param>
    /// <returns>The domain object</returns>
    Task<T> GetAsync(IDbContext context, int id);

    /// <summary>
    /// Add a new domain object to the repository
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="item">The domain object to add</param>
    /// <returns>The newly created domain object including the new id</returns>
    Task<T> CreateAsync(IDbContext context, T item);

    /// <summary>
    /// Add multiple new domain objects to the repository
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="items">The domain objects to add</param>
    /// <returns>The newly created domain objects including the new id</returns>
    Task<IEnumerable<T>> CreateAsync(IDbContext context, IEnumerable<T> items);

    /// <summary>
    /// Update a repository domain object
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="obj">The domain object to update</param>
    /// <returns>The updated domain object</returns>
    Task<T> UpdateAsync(IDbContext context, T obj);

    /// <summary>
    /// Remove a domain object from the repository
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="id">The if of the domain object to delete</param>
    /// <returns>True if the record was deleted</returns>
    Task<bool> DeleteAsync(IDbContext context, int id);
}