using System.Threading.Tasks;

namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Generic domain item repository with support of the basic CRUD operations
/// </summary>
public interface IDomainRepository : IRepository
{
    #region Basic Operations

    /// <summary>
    /// Test if domain item exists
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="id">The item id</param>
    /// <returns>True, if the domain item exists</returns>
    Task<bool> ExistsAsync(IDbContext context, int id);

    /// <summary>
    /// Test if domain item exists
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="fieldName">The item field name</param>
    /// <param name="value">The item field value</param>
    /// <returns>True, if the domain item exists</returns>
    Task<bool> ExistsAsync(IDbContext context, string fieldName, object value);

    #endregion

    #region Attributes

    /// <summary>
    /// Gets attribute value as JSON
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="id">The item id</param>
    /// <param name="attributeName">The attribute name</param>
    /// <returns>The attribute value</returns>
    Task<string> GetAttributeAsync(IDbContext context, int id, string attributeName);

    /// <summary>
    /// Test if an attribute exists
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="id">The item id</param>
    /// <param name="attributeName">The attribute name</param>
    /// <returns>True, if the attribute exists</returns>
    Task<bool> ExistsAttributeAsync(IDbContext context, int id, string attributeName);

    /// <summary>
    /// Creates or updates the attribute value
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="id">The item id</param>
    /// <param name="attributeName">The attribute name</param>
    /// <param name="value">The attribute value as JSON</param>
    Task<string> SetAttributeAsync(IDbContext context, int id, string attributeName, string value);

    /// <summary>
    /// Delete an attribute
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="id">The id of the domain item to delete</param>
    /// <param name="attributeName">The attribute name</param>
    Task<bool?> DeleteAttributeAsync(IDbContext context, int id, string attributeName);

    #endregion
}