using System.Threading.Tasks;

namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Repository for lookup sets
/// </summary>
public interface ILookupSetRepository : ILookupRepository<LookupSet>
{
    /// <summary>
    /// Get a lookup set, including the lookup values
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="lookupId">The lookup id</param>
    /// <returns>The lookup set</returns>
    Task<LookupSet> GetSetAsync(int tenantId, int regulationId, int lookupId);

    /// <summary>
    /// Get lookup values in a specific language
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="lookupId">The id of the lookup</param>
    /// <param name="language">The content language</param>
    Task<LookupData> GetLookupDataAsync(int tenantId, int regulationId, int lookupId, Language? language = null);

    /// <summary>
    /// Get lookup values in a specific language
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="lookupId">The id of the lookup</param>
    /// <param name="lookupKey">The lookup key</param>
    /// <param name="language">The content language</param>
    Task<LookupValueData> GetLookupValueDataAsync(int tenantId, int lookupId, string lookupKey, Language? language = null);

    /// <summary>
    /// Get lookup values in a specific language
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="lookupId">The id of the lookup</param>
    /// <param name="rangeValue">The range value</param>
    /// <param name="lookupKey">The lookup key</param>
    /// <param name="language">The content language</param>
    Task<LookupValueData> GetRangeLookupValueDataAsync(int tenantId, int lookupId, decimal rangeValue, string lookupKey, Language? language = null);
}