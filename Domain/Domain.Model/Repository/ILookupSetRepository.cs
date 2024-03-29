﻿using System.Threading.Tasks;

namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Repository for lookup sets
/// </summary>
public interface ILookupSetRepository : ILookupRepository<LookupSet>
{
    /// <summary>
    /// Get a lookup set, including the lookup values
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="lookupId">The lookup id</param>
    /// <returns>The lookup set</returns>
    // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Global
    Task<LookupSet> GetLookupSetAsync(IDbContext context, int tenantId, int regulationId, int lookupId);

    /// <summary>
    /// Get lookup values in a specific language
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="lookupId">The id of the lookup</param>
    /// <param name="culture">The content culture</param>
    Task<LookupData> GetLookupDataAsync(IDbContext context, int tenantId, int regulationId, int lookupId, string culture = null);

    /// <summary>
    /// Get lookup values in a specific language
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="lookupId">The id of the lookup</param>
    /// <param name="lookupKey">The lookup key</param>
    /// <param name="culture">The content culture</param>
    // ReSharper disable once UnusedParameter.Global
    Task<LookupValueData> GetLookupValueDataAsync(IDbContext context, int tenantId, int lookupId, string lookupKey, string culture = null);

    /// <summary>
    /// Get lookup values in a specific language
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="lookupId">The id of the lookup</param>
    /// <param name="rangeValue">The range value</param>
    /// <param name="lookupKey">The lookup key</param>
    /// <param name="culture">The content culture</param>
    // ReSharper disable once UnusedParameter.Global
    Task<LookupValueData> GetRangeLookupValueDataAsync(IDbContext context, int tenantId, int lookupId, decimal rangeValue, string lookupKey, string culture = null);
}