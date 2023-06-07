using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Scripting;

/// <summary>
/// Provides a lookup
/// </summary>
public sealed class RegulationLookupProvider : IRegulationLookupProvider
{
    /// <summary>
    /// The derived lookup
    /// </summary>
    private ILookup<string, Lookup> DerivedLookups { get; }

    /// <summary>
    /// The regulation repository
    /// </summary>
    private IRegulationRepository RegulationRepository { get; }

    /// <summary>
    /// The lookup repository
    /// </summary>
    private ILookupSetRepository LookupSetRepository { get; }

    /// <summary>
    /// Constructor for national time value provider
    /// </summary>
    public RegulationLookupProvider(IEnumerable<Lookup> lookups,
        IRegulationRepository regulationRepository, ILookupSetRepository lookupSetRepository) :
        this(lookups.ToLookup(col => col.Name, col => col),
            regulationRepository, lookupSetRepository)
    {
    }

    /// <summary>
    /// Constructor for national time value provider
    /// </summary>
    public RegulationLookupProvider(ILookup<string, Lookup> derivedLookups,
        IRegulationRepository regulationRepository, ILookupSetRepository lookupSetRepository)
    {
        DerivedLookups = derivedLookups ?? throw new ArgumentNullException(nameof(derivedLookups));
        RegulationRepository = regulationRepository ?? throw new ArgumentNullException(nameof(regulationRepository));
        LookupSetRepository = lookupSetRepository ?? throw new ArgumentNullException(nameof(lookupSetRepository));
    }

    /// <summary>
    /// Get a derived lookup value
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="lookupName">The name of the lookup</param>
    /// <param name="lookupKey">The lookup value key</param>
    /// <param name="language">The value language</param>
    /// <returns>The lookup value</returns>
    public async Task<LookupValueData> GetLookupValueDataAsync(IDbContext context, string lookupName, string lookupKey,
        Language? language = null)
    {
        if (string.IsNullOrWhiteSpace(lookupName))
        {
            throw new ArgumentException(nameof(lookupName));
        }
        if (!DerivedLookups.Contains(lookupName))
        {
            return null;
        }
        if (string.IsNullOrWhiteSpace(lookupKey))
        {
            throw new ArgumentException(nameof(lookupKey));
        }

        // unknown lookup
        if (!DerivedLookups[lookupName].Any())
        {
            throw new ArgumentException($"Missing lookup with name {lookupName}");
        }

        // derived lookups
        foreach (var lookup in DerivedLookups[lookupName])
        {
            // lookup regulation
            var regulationId = await LookupSetRepository.GetParentIdAsync(context, lookup.Id);
            if (!regulationId.HasValue)
            {
                throw new ArgumentException($"Invalid lookup with name {lookupName}");
            }

            // lookup tenant
            var tenantId = await RegulationRepository.GetParentIdAsync(context, regulationId.Value);
            if (!tenantId.HasValue)
            {
                throw new ArgumentException($"Invalid lookup with name {lookupName}");
            }

            // lookup value
            var value = await LookupSetRepository.GetLookupValueDataAsync(context, tenantId.Value, lookup.Id, lookupKey, language);
            if (value != null)
            {
                return value;
            }
        }

        return null;
    }

    /// <summary>
    /// Get a derived range lookup value
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="lookupName">The name of the lookup</param>
    /// <param name="rangeValue">The range value</param>
    /// <param name="lookupKey">The lookup key</param>
    /// <param name="language">The value language</param>
    /// <returns>The lookup value</returns>
    public async Task<LookupValueData> GetRangeLookupValueDataAsync(IDbContext context,
        string lookupName, decimal rangeValue, string lookupKey = null, Language? language = null)
    {
        if (string.IsNullOrWhiteSpace(lookupName))
        {
            throw new ArgumentException(nameof(lookupName));
        }
        if (!DerivedLookups.Contains(lookupName))
        {
            return null;
        }

        // unknown lookup
        if (!DerivedLookups[lookupName].Any())
        {
            throw new ArgumentException($"Missing lookup with name {lookupName}");
        }

        // derived lookups
        foreach (var lookup in DerivedLookups[lookupName])
        {
            // lookup regulation
            var regulationId = await LookupSetRepository.GetParentIdAsync(context, lookup.Id);
            if (!regulationId.HasValue)
            {
                throw new ArgumentException($"Invalid lookup with name {lookupName}");
            }

            // lookup tenant
            var tenantId = await RegulationRepository.GetParentIdAsync(context, regulationId.Value);
            if (!tenantId.HasValue)
            {
                throw new ArgumentException($"Invalid lookup with name {lookupName}");
            }

            // lookup value
            var value = await LookupSetRepository.GetRangeLookupValueDataAsync(context, tenantId.Value, lookup.Id, rangeValue, lookupKey, language);
            if (value != null)
            {
                return value;
            }
        }

        return null;
    }
}