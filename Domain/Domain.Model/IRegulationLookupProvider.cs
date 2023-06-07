using System.Threading.Tasks;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Provides a lookup
/// </summary>
public interface IRegulationLookupProvider
{
    /// <summary>
    /// Get a derived lookup value
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="lookupName">The name of the lookup</param>
    /// <param name="lookupKey">The lookup value key</param>
    /// <param name="language">The value language</param>
    /// <returns>The lookup value</returns>
    Task<LookupValueData> GetLookupValueDataAsync(IDbContext context, string lookupName, string lookupKey,
        Language? language = null);

    /// <summary>
    /// Get a derived range lookup value
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="lookupName">The name of the lookup</param>
    /// <param name="rangeValue">The range value</param>
    /// <param name="lookupKey">The lookup key</param>
    /// <param name="language">The value language</param>
    /// <returns>The lookup value</returns>
    Task<LookupValueData> GetRangeLookupValueDataAsync(IDbContext context,
        string lookupName, decimal rangeValue, string lookupKey = null, Language? language = null);
}