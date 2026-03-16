using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Persistence.DbSchema;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Persistence;

public class LookupSetRepository(IRegulationRepository regulationRepository,
        ILookupValueRepository valueRepository, ILookupAuditRepository auditRepository, bool auditEnabled)
    : LookupRepositoryBase<LookupSet>(regulationRepository, auditRepository, auditEnabled), ILookupSetRepository
{
    private ILookupValueRepository ValueRepository { get; } = valueRepository ?? throw new ArgumentNullException(nameof(valueRepository));

    protected override async Task OnRetrieved(IDbContext context, int regulationId, LookupSet lookupSet)
    {
        // lookup values
        if (lookupSet != null)
        {
            lookupSet.Values = (await ValueRepository.QueryAsync(context, lookupSet.Id)).ToList();
        }
    }

    public async Task<LookupSet> GetLookupSetAsync(IDbContext context, int regulationId, int lookupId)
    {
        if (regulationId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(regulationId));
        }
        if (lookupId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lookupId));
        }

        // lookup
        var lookupSet = await GetAsync(context, regulationId, lookupId);
        if (lookupSet != null)
        {
            // lookup values
            lookupSet.Values = (await ValueRepository.QueryAsync(context, lookupId)).ToList();
        }

        return lookupSet;
    }

    public override async Task<IEnumerable<LookupSet>> CreateAsync(IDbContext context, int regulationId, IEnumerable<LookupSet> items)
    {
        if (items == null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        var @namespace = await GetRegulationNamespaceAsync(context, regulationId);
        var lookups = new List<LookupSet>();
        var newLookups = new List<LookupSet>();
        foreach (var item in items)
        {
            item.Name = item.Name.EnsureNamespace(@namespace);
            var existing = await QueryLookupSetAsync(context, regulationId, item.Name);
            if (existing != null)
            {
                item.Id = existing.Id;
                lookups.Add(item);
            }
            else
            {
                if (item.Values != null)
                {
                    foreach (var value in item.Values)
                    {
                        value.InitCreatedDate(item.Created);
                    }
                }
                // clear lookup value id for new lookup to avoid confusion
                newLookups.Add(item);
            }
        }

        // transaction
        using (var txScope = TransactionFactory.NewTransactionScope())
        {
            // create lookups
            if (newLookups.Any())
            {
                var created = (await base.CreateAsync(context, regulationId, newLookups)).ToList();
                if (created.Any())
                {
                    lookups.AddRange(created);
                }
            }

            // create lookup values for each lookup
            // group by id to avoid duplicate inserts when the same lookup appears
            // multiple times in the input (e.g. same name under different regulations)
            var lookupById = lookups
                .GroupBy(x => x.Id)
                .Select(g =>
                {
                    var first = g.First();
                    // merge values from all entries with the same id
                    first.Values = g.SelectMany(x => x.Values ?? [])
                                    .DistinctBy(v => (v.RangeValue, v.LookupHash))
                                    .ToList();
                    return first;
                })
                .ToList();

            foreach (var lookup in lookupById)
            {
                if (lookup.Values == null || !lookup.Values.Any())
                {
                    continue;
                }

                // delete existing values before re-inserting (clean-replace pattern)
                await ValueRepository.DeleteAll(context, lookup.Id);

                // performance optimization: insert lookup values with bulk mode
                await ValueRepository.CreateBulkAsync(context, lookup.Id, lookup.Values);
            }

            txScope.Complete();
        }

        // refresh query optimizer statistics after bulk import to prevent
        // plan degradation (tipping point) on large lookup value sets
        // must run outside the TransactionScope (scope must be disposed first)
        await context.UpdateStatisticsTargetedAsync();

        return lookups;
    }

    private async Task<LookupSet> QueryLookupSetAsync(IDbContext context, int regulationId, string name)
    {
        // query
        var query = DbQueryFactory.NewQuery(TableName, ParentFieldName, regulationId);

        // filter by lookup name
        query.WhereIn(LookupColumn.Name, name);

        // execute query
        var compileQuery = CompileQuery(query, context);
        return (await QueryAsync(context, compileQuery)).FirstOrDefault();
    }

    public async Task<LookupData> GetLookupDataAsync(IDbContext context,
        int regulationId, int lookupId, string culture = null)
    {
        var lookupData = new LookupData();

        // lookup set
        var lookupSet = await GetLookupSetAsync(context, regulationId, lookupId);
        if (lookupSet == null)
        {
            return lookupData;
        }
        lookupData.Name = lookupSet.Name;
        lookupData.RangeSize = lookupSet.RangeSize;
        lookupData.Culture = culture;

        // lookup value localizations
        foreach (var value in lookupSet.Values)
        {
            var dataValue = value.Value;
            if (!string.IsNullOrWhiteSpace(culture))
            {
                dataValue = culture.GetLocalization(value.ValueLocalizations, value.Value);
            }

            lookupData.Values ??= [];
            var valueData = new LookupValueData
            {
                Key = value.Key,
                Value = dataValue,
                RangeValue = value.RangeValue,
            };
            lookupData.Values.Add(valueData);
        }

        return lookupData;
    }

    public async Task<LookupValueData> GetLookupValueDataAsync(IDbContext context,
        int lookupId, string lookupKey, string culture = null)
    {
        if (lookupId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lookupId));
        }
        if (string.IsNullOrWhiteSpace(lookupKey))
        {
            throw new ArgumentException(nameof(lookupKey));
        }

        // select lookup value lookup-id and key-hash-code
        var lookupValue = await SelectSingleAsync<LookupValue>(context, Tables.LookupValue,
            new()
            {
                { LookupValueColumn.LookupId, lookupId },
                { LookupValueColumn.KeyHash, lookupKey.ToPayrollHash() }
            });
        if (lookupValue == null)
        {
            return null;
        }

        // lookup value localizations
        var dataValue = lookupValue.Value;
        if (!string.IsNullOrWhiteSpace(culture))
        {
            dataValue = culture.GetLocalization(lookupValue.ValueLocalizations, lookupValue.Value);
        }

        return new()
        {
            Key = lookupKey,
            Value = dataValue,
            RangeValue = lookupValue.RangeValue
        };
    }

    public async Task<LookupValueData> GetRangeLookupValueDataAsync(IDbContext context,
        int lookupId, decimal rangeValue, string lookupKey = null, string culture = null)
    {
        if (lookupId <= 0)
        {
            throw new ArgumentException(nameof(lookupId));
        }

        var parameters = new DbParameterCollection();
        parameters.Add(ParameterGetLookupRangeValue.LookupId, lookupId, DbType.Int32);
        parameters.Add(ParameterGetLookupRangeValue.RangeValue, rangeValue, DbType.Decimal);
        // always pass KeyHash — null means no key filter (SP handles NULL)
        parameters.Add(ParameterGetLookupRangeValue.KeyHash,
            lookupKey?.ToPayrollHash(), DbType.Int32);

        // retrieve all derived lookups (stored procedure)
        var lookupValue = (await QueryAsync<LookupValue>(context, Procedures.GetLookupRangeValue,
            parameters, commandType: CommandType.StoredProcedure)).FirstOrDefault();
        if (lookupValue == null)
        {
            return null;
        }

        // lookup value localizations
        var dataValue = lookupValue.Value;
        if (!string.IsNullOrWhiteSpace(culture))
        {
            dataValue = culture.GetLocalization(lookupValue.ValueLocalizations, lookupValue.Value);
        }

        return new()
        {
            Key = lookupValue.Key,
            Value = dataValue,
            RangeValue = lookupValue.RangeValue
        };
    }
}