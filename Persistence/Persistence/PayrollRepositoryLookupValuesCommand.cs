using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence;

internal sealed class PayrollRepositoryLookupValuesCommand : PayrollRepositoryCommandBase
{
    internal PayrollRepositoryLookupValuesCommand(IDbContext dbContext) :
        base(dbContext)
    {
    }

    internal async Task<IEnumerable<DerivedLookupValue>> GetDerivedLookupValuesAsync(PayrollQuery query,
        IEnumerable<string> lookupNames = null, IEnumerable<string> lookupKeys = null,
        OverrideType? overrideType = null)
    {
        // query check
        if (query == null)
        {
            throw new ArgumentNullException(nameof(query));
        }
        if (query.TenantId <= 0)
        {
            throw new ArgumentException(nameof(query.TenantId));
        }
        if (query.PayrollId <= 0)
        {
            throw new ArgumentException(nameof(query.PayrollId));
        }

        // lookup filter
        var names = lookupNames?.Distinct().ToList();
        if (names != null)
        {
            foreach (var name in names)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException(nameof(lookupNames));
                }
            }
        }

        // lookup value filter
        var keys = lookupKeys?.Distinct().ToList();
        if (keys != null)
        {
            foreach (var key in keys)
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    throw new ArgumentException(nameof(lookupKeys));
                }
            }
        }

        // query setup
        query.RegulationDate ??= Date.Now;
        query.EvaluationDate ??= Date.Now;

        // parameters
        var parameters = new DbParameterCollection();
        parameters.Add(DbSchema.ParameterGetDerivedLookupValues.TenantId, query.TenantId, DbType.Int32);
        parameters.Add(DbSchema.ParameterGetDerivedLookupValues.PayrollId, query.PayrollId, DbType.Int32);
        parameters.Add(DbSchema.ParameterGetDerivedLookupValues.RegulationDate, query.RegulationDate, DbType.DateTime2);
        parameters.Add(DbSchema.ParameterGetDerivedLookupValues.CreatedBefore, query.EvaluationDate, DbType.DateTime2);
        if (names != null && names.Any())
        {
            parameters.Add(DbSchema.ParameterGetDerivedLookupValues.LookupNames,
                System.Text.Json.JsonSerializer.Serialize(names));
        }
        if (keys != null && keys.Any())
        {
            parameters.Add(DbSchema.ParameterGetDerivedLookupValues.LookupKeys,
                System.Text.Json.JsonSerializer.Serialize(keys));
        }

        // retrieve all derived lookup parameters (stored procedure)
        var lookupParameters = (await DbContext.QueryAsync<DerivedLookupValue>(DbSchema.Procedures.GetDerivedLookupValues,
            parameters, commandType: CommandType.StoredProcedure)).ToList();

        BuildDerivedLookupValues(lookupParameters, overrideType);
        return lookupParameters;
    }

    private static void BuildDerivedLookupValues(List<DerivedLookupValue> lookupParameters, OverrideType? overrideType = null)
    {
        if (lookupParameters == null)
        {
            throw new ArgumentNullException(nameof(lookupParameters));
        }
        if (!lookupParameters.Any())
        {
            return;
        }

        // resulting lookups
        var lookupParametersByKey = lookupParameters.GroupBy(x => x.Key).ToList();

        // override filter
        if (overrideType.HasValue)
        {
            ApplyOverrideFilter(lookupParametersByKey, lookupParameters, overrideType.Value);
            // update lookups
            lookupParametersByKey = lookupParameters.GroupBy(x => x.Key).ToList();
        }

        // collect derived values
        foreach (var lookupKey in lookupParametersByKey)
        {
            // order by derived lookups
            var derivedParameters = lookupKey.OrderByDescending(x => x.Level).ThenByDescending(x => x.Priority).ToList();
            // derived lookups
            while (derivedParameters.Count > 1)
            {
                // collect derived values
                var derivedParameter = derivedParameters.First();
                // non-derived fields: key
                derivedParameter.RangeValue = CollectDerivedValue(derivedParameters, x => x.RangeValue);
                derivedParameter.Value = CollectDerivedValue(derivedParameters, x => x.Value);
                derivedParameter.ValueLocalizations = CollectDerivedValue(derivedParameters, x => x.ValueLocalizations);
                // remove the current level for the next iteration
                derivedParameters.Remove(derivedParameter);
            }
        }
    }
}