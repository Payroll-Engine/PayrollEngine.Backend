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

    /// <inheritdoc />
    public Task<bool> HasLookupAsync(IDbContext context, string lookupName) =>
        System.Threading.Tasks.Task.FromResult(DerivedLookups.Contains(lookupName));

    /// <inheritdoc />
    public async Task<LookupValueData> GetLookupValueDataAsync(IDbContext context, string lookupName, string lookupKey,
        string culture = null)
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
            throw new ArgumentException($"Missing lookup with name {lookupName}.");
        }

        // derived lookups
        foreach (var lookup in DerivedLookups[lookupName])
        {
            // lookup regulation
            var regulationId = await LookupSetRepository.GetParentIdAsync(context, lookup.Id);
            if (!regulationId.HasValue)
            {
                throw new ArgumentException($"Invalid lookup with name {lookupName}.");
            }

            // lookup tenant
            var tenantId = await RegulationRepository.GetParentIdAsync(context, regulationId.Value);
            if (!tenantId.HasValue)
            {
                throw new ArgumentException($"Invalid lookup with name {lookupName}.");
            }

            // lookup value
            var value = await LookupSetRepository.GetLookupValueDataAsync(context, lookup.Id, lookupKey, culture);
            if (value != null)
            {
                return value;
            }
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<LookupValueData> GetRangeLookupValueDataAsync(IDbContext context,
        string lookupName, decimal rangeValue, string lookupKey = null, string culture = null)
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
            throw new ArgumentException($"Missing lookup with name {lookupName}.");
        }

        // derived lookups
        foreach (var lookup in DerivedLookups[lookupName])
        {
            // lookup regulation
            var regulationId = await LookupSetRepository.GetParentIdAsync(context, lookup.Id);
            if (!regulationId.HasValue)
            {
                throw new ArgumentException($"Invalid lookup with name {lookupName}.");
            }

            // lookup tenant
            var tenantId = await RegulationRepository.GetParentIdAsync(context, regulationId.Value);
            if (!tenantId.HasValue)
            {
                throw new ArgumentException($"Invalid lookup with name {lookupName}.");
            }

            // lookup value
            var value = await LookupSetRepository.GetRangeLookupValueDataAsync(context, lookup.Id, rangeValue, lookupKey, culture);
            if (value != null)
            {
                return value;
            }
        }

        return null;
    }
}