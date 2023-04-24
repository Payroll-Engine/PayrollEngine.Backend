using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Persistence;

public class LookupSetRepository : LookupRepositoryBase<LookupSet>, ILookupSetRepository
{
    public ILookupValueRepository ValueRepository { get; }

    public LookupSetRepository(ILookupValueRepository valueRepository,
        ILookupAuditRepository auditRepository) :
        base(auditRepository)
    {
        ValueRepository = valueRepository ?? throw new ArgumentNullException(nameof(valueRepository));
    }

    protected override async Task OnRetrieved(IDbContext context, int regulationId, LookupSet lookupSet)
    {
        // lookup values
        if (lookupSet != null)
        {
            lookupSet.Values = (await ValueRepository.QueryAsync(context, lookupSet.Id)).ToList();
        }
    }

    public virtual async Task<LookupSet> GetSetAsync(IDbContext context, int tenantId, int regulationId, int lookupId)
    {
        if (tenantId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tenantId));
        }
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

        // transaction
        using var txScope = TransactionFactory.NewTransactionScope();

        // create lookups
        var createdLookups = (await base.CreateAsync(context, regulationId, items)).ToList();

        // create lookup values for each created lookup
        foreach (var createdLookup in createdLookups)
        {
            // performance optimization: insert case values with bulk mode
            await ValueRepository.CreateBulkAsync(context, createdLookup.Id, createdLookup.Values);
        }

        txScope.Complete();
        return createdLookups;
    }

    public override async Task<bool> DeleteAsync(IDbContext context, int regulationId, int lookupId)
    {
        using var txScope = TransactionFactory.NewTransactionScope();

        // lookup values
        await ValueRepository.DeleteAll(context, lookupId);

        // lookup
        var deleted = await base.DeleteAsync(context, regulationId, lookupId);

        txScope.Complete();

        return deleted;
    }

    public virtual async Task<LookupData> GetLookupDataAsync(IDbContext context,
        int tenantId, int regulationId, int lookupId, Language? language = null)
    {
        var lookupData = new LookupData();

        // lookup set
        var lookupSet = await GetSetAsync(context, tenantId, regulationId, lookupId);
        if (lookupSet == null)
        {
            return lookupData;
        }
        lookupData.Name = lookupSet.Name;
        lookupData.RangeSize = lookupSet.RangeSize;
        lookupData.Language = language;

        // lookup value localizations
        foreach (var value in lookupSet.Values)
        {
            var dataValue = value.Value;
            if (language.HasValue)
            {
                dataValue = language.Value.GetLocalization(value.ValueLocalizations, value.Value);
            }

            lookupData.Values ??= new();
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
        int tenantId, int lookupId, string lookupKey, Language? language = null)
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
        if (language.HasValue)
        {
            dataValue = language.Value.GetLocalization(lookupValue.ValueLocalizations, lookupValue.Value);
        }

        return new()
        {
            Key = lookupKey,
            Value = dataValue,
            RangeValue = lookupValue.RangeValue
        };
    }

    public virtual async Task<LookupValueData> GetRangeLookupValueDataAsync(IDbContext context,
        int tenantId, int lookupId, decimal rangeValue, string lookupKey = null, Language? language = null)
    {
        if (lookupId <= 0)
        {
            throw new ArgumentException(nameof(lookupId));
        }

        var parameters = new DbParameterCollection();
        parameters.Add(DbSchema.ParameterGetLookupRangeValue.LookupId, lookupId);
        parameters.Add(DbSchema.ParameterGetLookupRangeValue.RangeValue, rangeValue);
        if (lookupKey != null)
        {
            // filter by lookup key without the range value
            parameters.Add(DbSchema.ParameterGetLookupRangeValue.KeyHash, lookupKey.ToPayrollHash());
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
        if (language.HasValue)
        {
            dataValue = language.Value.GetLocalization(lookupValue.ValueLocalizations, lookupValue.Value);
        }

        return new()
        {
            Key = lookupValue.Key,
            Value = dataValue,
            RangeValue = lookupValue.RangeValue
        };
    }
}