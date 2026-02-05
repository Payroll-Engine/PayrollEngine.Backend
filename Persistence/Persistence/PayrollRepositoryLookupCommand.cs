using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence;

internal sealed class PayrollRepositoryLookupCommand : PayrollRepositoryCommandBase
{
    internal PayrollRepositoryLookupCommand(IDbContext dbContext) :
        base(dbContext)
    {
    }

    internal async Task<IEnumerable<DerivedLookup>> GetDerivedLookupsAsync(PayrollQuery query,
        IEnumerable<string> lookupNames = null, OverrideType? overrideType = null)
    {
        // query validation
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

        // query setup
        query.RegulationDate ??= Date.Now;
        query.EvaluationDate ??= Date.Now;

        // retrieve all derived lookups (stored procedure)
        var parameters = new DbParameterCollection();
        parameters.Add(DbSchema.ParameterGetDerivedLookups.TenantId, query.TenantId, DbType.Int32);
        parameters.Add(DbSchema.ParameterGetDerivedLookups.PayrollId, query.PayrollId, DbType.Int32);
        parameters.Add(DbSchema.ParameterGetDerivedLookups.RegulationDate, query.RegulationDate, DbType.DateTime2);
        parameters.Add(DbSchema.ParameterGetDerivedLookups.CreatedBefore, query.EvaluationDate, DbType.DateTime2);
        if (names != null && names.Any())
        {
            parameters.Add(DbSchema.ParameterGetDerivedLookups.LookupNames,
                System.Text.Json.JsonSerializer.Serialize(names));
        }
        var lookups = (await DbContext.QueryAsync<DerivedLookup>(DbSchema.Procedures.GetDerivedLookups,
            parameters, commandType: CommandType.StoredProcedure)).ToList();

        // consolidation
        if (lookups.Any())
        {
            var lookupsByKeys = lookups.GroupBy(x => x.Name).ToList();

            // override filter
            if (overrideType.HasValue)
            {
                ApplyOverrideFilter(lookupsByKeys, lookups, overrideType.Value);
            }
        }

        return lookups;
    }
}