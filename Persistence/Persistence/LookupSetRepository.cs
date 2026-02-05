using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Task = System.Threading.Tasks.Task;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class LookupSetRepository(IRegulationRepository regulationRepository,
        ILookupValueRepository valueRepository, ILookupAuditRepository auditRepository, bool auditDisabled)
    : LookupRepositoryBase<LookupSet>(regulationRepository, auditRepository, auditDisabled), ILookupSetRepository
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
                newLookups.Add(item);
            }
        }

        // transaction
        using var txScope = TransactionFactory.NewTransactionScope();

        // create lookups
        if (newLookups.Any())
        {
            var created = (await base.CreateAsync(context, regulationId, newLookups)).ToList();
            if (created.Any())
            {
                lookups.AddRange(created);
            }
        }

        // create lookup values for each created lookup
        foreach (var lookup in lookups)
        {
            // performance optimization: insert case values with bulk mode
            await ValueRepository.CreateBulkAsync(context, lookup.Id, lookup.Values);
        }

        txScope.Complete();
        return lookups;
    }

    private async Task<LookupSet> QueryLookupSetAsync(IDbContext context, int regulationId, string name)
    {
        // query
        var query = DbQueryFactory.NewQuery(TableName, ParentFieldName, regulationId);

        // filter by lookup name
        query.WhereIn(DbSchema.LookupColumn.Name, name);

        // execute query
        var compileQuery = CompileQuery(query);
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
        var lookupValue = await SelectSingleAsync<LookupValue>(context, DbSchema.Tables.LookupValue,
            new()
            {
                { DbSchema.LookupValueColumn.LookupId, lookupId },
                { DbSchema.LookupValueColumn.KeyHash, lookupKey.ToPayrollHash() }
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
        parameters.Add(DbSchema.ParameterGetLookupRangeValue.LookupId, lookupId, DbType.Int32);
        parameters.Add(DbSchema.ParameterGetLookupRangeValue.RangeValue, rangeValue, DbType.Decimal);
        if (lookupKey != null)
        {
            // filter by lookup key without the range value
            parameters.Add(DbSchema.ParameterGetLookupRangeValue.KeyHash, lookupKey.ToPayrollHash(), DbType.Int32);
        }

        // retrieve all derived lookups (stored procedure)
        var lookupValue = (await QueryAsync<LookupValue>(context, DbSchema.Procedures.GetLookupRangeValue,
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