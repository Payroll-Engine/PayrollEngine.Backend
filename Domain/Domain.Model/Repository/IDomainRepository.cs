using System.Threading.Tasks;

namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Generic domain object repository with support of the basic CRUD operations
/// </summary>
public interface IDomainRepository : IRepository
{
    #region Basic Operations

    /// <summary>
    /// Test if an domain object exists
    /// </summary>
    /// <param name="id">The object id</param>
    /// <returns>True, if the domain object exists</returns>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// Test if an domain object exists
    /// </summary>
    /// <param name="fieldName">The object field name</param>
    /// <param name="value">The object field value</param>
    /// <returns>True, if the domain object exists</returns>
    Task<bool> ExistsAsync(string fieldName, object value);

    #endregion

    #region Attributes

    /// <summary>
    /// Gets attribute value as JSON
    /// </summary>
    /// <param name="id">The object id</param>
    /// <param name="attributeName">The attribute name</param>
    /// <returns>The attribute value</returns>
    Task<string> GetAttributeAsync(int id, string attributeName);

    /// <summary>
    /// Test if an attribute exists
    /// </summary>
    /// <param name="id">The object id</param>
    /// <param name="attributeName">The attribute name</param>
    /// <returns>True, if the attribute exists</returns>
    Task<bool> ExistsAttributeAsync(int id, string attributeName);

    /// <summary>
    /// Creates or updates the attribute value
    /// </summary>
    /// <param name="id">The object id</param>
    /// <param name="attributeName">The attribute name</param>
    /// <param name="value">The attribute value as JSON</param>
    Task<string> SetAttributeAsync(int id, string attributeName, string value);

    /// <summary>
    /// Delete an attribute
    /// </summary>
    /// <param name="id">The if of the domain object to delete</param>
    /// <param name="attributeName">The attribute name</param>
    Task<bool?> DeleteAttributeAsync(int id, string attributeName);

    #endregion
}