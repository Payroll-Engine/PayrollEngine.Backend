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
    private ILookup<string, Lookup> Lookups { get; set; }

    private IDbContext DbContext { get; }
    private IPayrollRepository PayrollRepository { get; }
    private PayrollQuery PayrollQuery { get; }
    private IRegulationRepository RegulationRepository { get; }
    private ILookupSetRepository LookupSetRepository { get; }

    /// <summary>
    /// Constructor for national time value provider
    /// </summary>
    public RegulationLookupProvider(IDbContext dbContext,
        IPayrollRepository payrollRepository, PayrollQuery payrollQuery,
        IRegulationRepository regulationRepository,
        ILookupSetRepository lookupSetRepository)
    {
        DbContext = dbContext;
        PayrollRepository = payrollRepository;
        PayrollQuery = payrollQuery;
        RegulationRepository = regulationRepository ?? throw new ArgumentNullException(nameof(regulationRepository));
        LookupSetRepository = lookupSetRepository ?? throw new ArgumentNullException(nameof(lookupSetRepository));
    }

    /// <inheritdoc />
    public async Task<bool> HasLookupAsync(IDbContext context, string lookupName)
    {
        var lookups = await GetLookupsAsync();
        return lookups.Contains(lookupName);
    }

    /// <inheritdoc />
    public async Task<LookupSet> GetLookupAsync(IDbContext context, string lookupName)
    {
        // lookup
        var lookups = await GetLookupsAsync();
        if (!lookups.Contains(lookupName))
        {
            return null;
        }
        var lookup = new LookupSet(lookups[lookupName].First());

        // lookup values
        var values = await GetLookupValuesAsync(lookup.Name);
        if (values.Any())
        {
            lookup.Values ??= [];
            lookup.Values.AddRange(values);
        }

        return lookup;
    }

    /// <inheritdoc />
    public async Task<LookupValueData> GetLookupValueDataAsync(IDbContext context, string lookupName, string lookupKey,
        string culture = null)
    {
        if (string.IsNullOrWhiteSpace(lookupName))
        {
            throw new ArgumentException(nameof(lookupName));
        }
        var lookups = await GetLookupsAsync();
        if (!lookups.Contains(lookupName))
        {
            return null;
        }
        if (string.IsNullOrWhiteSpace(lookupKey))
        {
            throw new ArgumentException(nameof(lookupKey));
        }

        // unknown lookup
        if (!lookups[lookupName].Any())
        {
            throw new ArgumentException($"Missing lookup with name {lookupName}.");
        }

        // derived lookups
        foreach (var lookup in lookups[lookupName])
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
        var lookups = await GetLookupsAsync();
        if (!lookups.Contains(lookupName))
        {
            return null;
        }

        // unknown lookup
        if (!lookups[lookupName].Any())
        {
            throw new ArgumentException($"Missing lookup with name {lookupName}.");
        }

        // derived lookups
        foreach (var lookup in lookups[lookupName])
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

    private async Task<ILookup<string, Lookup>> GetLookupsAsync()
    {
        if (Lookups == null)
        {
            var lookups = (await PayrollRepository.GetDerivedLookupsAsync(DbContext,
                PayrollQuery,
                overrideType: OverrideType.Active)).ToList();
            Lookups = lookups.Cast<Lookup>().ToLookup(col => col.Name, col => col);
        }
        return Lookups;
    }

    private async Task<List<LookupValue>> GetLookupValuesAsync(string lookupName)
    {
        var lookupValues = (await PayrollRepository.GetDerivedLookupValuesAsync(DbContext,
            PayrollQuery,
            lookupNames: [lookupName],
            overrideType: OverrideType.Active)).ToList();
        return lookupValues.Cast<LookupValue>().ToList();
    }
}