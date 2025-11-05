using System.Threading.Tasks;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Provides a lookup
/// </summary>
public interface IRegulationLookupProvider
{
    /// <summary>
    /// Test for existing lookup
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="lookupName">The name of the lookup</param>
    /// <returns>True for existing lookup</returns>
    // ReSharper disable once UnusedParameter.Global
    Task<bool> HasLookupAsync(IDbContext context, string lookupName);

    /// <summary>
    /// Get lookup
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="lookupName">The name of the lookup</param>
    /// <returns>The lookup</returns>
    // ReSharper disable once UnusedParameter.Global
    Task<LookupSet> GetLookupAsync(IDbContext context, string lookupName);

    /// <summary>
    /// Get a derived lookup value
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="lookupName">The name of the lookup</param>
    /// <param name="lookupKey">The lookup value key</param>
    /// <param name="culture">The value culture</param>
    /// <returns>The lookup value</returns>
    Task<LookupValueData> GetLookupValueDataAsync(IDbContext context, string lookupName, string lookupKey,
        string culture = null);

    /// <summary>
    /// Get a derived range lookup value
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="lookupName">The name of the lookup</param>
    /// <param name="rangeValue">The range value</param>
    /// <param name="lookupKey">The lookup key</param>
    /// <param name="culture">The value culture</param>
    /// <returns>The lookup value</returns>
    Task<LookupValueData> GetRangeLookupValueDataAsync(IDbContext context,
        string lookupName, decimal rangeValue, string lookupKey = null, string culture = null);
}