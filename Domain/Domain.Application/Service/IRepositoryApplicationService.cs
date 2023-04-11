using System.Threading.Tasks;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application.Service;

public interface IRepositoryApplicationService<out TRepo>
    where TRepo : class, IDomainRepository
{
    public TRepo Repository { get; }

    /// <summary>
    /// Test if item exists
    /// </summary>
    /// <param name="itemId">The item id</param>
    /// <returns>True if the item exists</returns>
    Task<bool> ExistsAsync(int itemId);

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