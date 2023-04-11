using System.Threading.Tasks;

namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Repository for lookup values
/// </summary>
public interface ILookupValueRepository : ITrackChildDomainRepository<LookupValue, LookupValueAudit>
{
    /// <summary>
    /// Determine if a lookup value with the same key exists
    /// </summary>
    /// <param name="lookupId">The lookup id</param>
    /// <param name="key">The lookup value key</param>
    /// <param name="rangeValue">The lookup range value</param>
    /// <returns>True if the lookup value with any of the key exists</returns>
    Task<bool> ExistsAsync(int lookupId, string key, decimal? rangeValue = null);

    /// <summary>
    /// Delete all lookup values from a lookup
    /// </summary>
    /// <param name="lookupId">The lookup id</param>
    /// <returns>The count of deleted lookup values</returns>
    Task<int> DeleteAll(int lookupId);
}