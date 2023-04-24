using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence;

internal sealed class PayrollRepositoryWageTypeCommand : PayrollRepositoryCommandBase
{

    internal PayrollRepositoryWageTypeCommand(IDbContext dbContext) :
        base(dbContext)
    {
    }

    internal async Task<IEnumerable<WageType>> GetDerivedWageTypesAsync(PayrollQuery query,
        IEnumerable<decimal> wageTypeNumbers = null,
        OverrideType? overrideType = null, ClusterSet clusterSet = null)
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
        var numbers = wageTypeNumbers?.Distinct().ToList();
        if (numbers != null)
        {
            foreach (var number in numbers)
            {
                if (number <= 0)
                {
                    throw new ArgumentException(nameof(wageTypeNumbers));
                }
            }
        }

        // query setup
        query.RegulationDate ??= Date.Now;
        query.EvaluationDate ??= Date.Now;

        // parameters
        var parameters = new DbParameterCollection();
        parameters.Add(DbSchema.ParameterGetDerivedWageTypes.TenantId, query.TenantId);
        parameters.Add(DbSchema.ParameterGetDerivedWageTypes.PayrollId, query.PayrollId);
        parameters.Add(DbSchema.ParameterGetDerivedWageTypes.RegulationDate, query.RegulationDate);
        parameters.Add(DbSchema.ParameterGetDerivedWageTypes.CreatedBefore, query.EvaluationDate);
        if (numbers != null && numbers.Any())
        {
            parameters.Add(DbSchema.ParameterGetDerivedWageTypes.WageTypeNumbers,
                System.Text.Json.JsonSerializer.Serialize(numbers));
        }
        if (clusterSet != null)
        {
            if (clusterSet.IncludeClusters != null && clusterSet.IncludeClusters.Any())
            {
                parameters.Add(DbSchema.ParameterGetDerivedWageTypes.IncludeClusters,
                    System.Text.Json.JsonSerializer.Serialize(clusterSet.IncludeClusters));
            }
            if (clusterSet.ExcludeClusters != null && clusterSet.ExcludeClusters.Any())
            {
                parameters.Add(DbSchema.ParameterGetDerivedWageTypes.ExcludeClusters,
                    System.Text.Json.JsonSerializer.Serialize(clusterSet.ExcludeClusters));
            }
        }

        // retrieve all wage types (stored procedure)
        var wageTypes = (await DbContext.QueryAsync<DerivedWageType>(DbSchema.Procedures.GetDerivedWageTypes,
            parameters, commandType: CommandType.StoredProcedure)).ToList();

        BuildDerivedWageTypes(wageTypes, overrideType);
        return wageTypes;
    }

    private static void BuildDerivedWageTypes(List<DerivedWageType> wageTypes, OverrideType? overrideType = null)
    {
        if (wageTypes == null)
        {
            throw new ArgumentNullException(nameof(wageTypes));
        }
        if (!wageTypes.Any())
        {
            return;
        }

        // resulting wage types
        var wageTypesByKey = wageTypes.GroupBy(x => x.WageTypeNumber).ToList();

        // override filter
        if (overrideType.HasValue)
        {
            ApplyOverrideFilter(wageTypesByKey, wageTypes, overrideType.Value);
            // update wage types
            wageTypesByKey = wageTypes.GroupBy(x => x.WageTypeNumber).ToList();
        }

        // collect derived values
        foreach (var wageTypeKey in wageTypesByKey)
        {
            // order by derived wage types
            var derivedWageTypes = wageTypeKey.OrderByDescending(x => x.Level).ThenByDescending(x => x.Priority).ToList();
            // derived wage types
            while (derivedWageTypes.Count > 1)
            {
                // collect derived values
                var derivedWageType = derivedWageTypes.First();
                derivedWageType.NameLocalizations = CollectDerivedValue(derivedWageTypes, x => x.NameLocalizations);
                derivedWageType.Description = CollectDerivedValue(derivedWageTypes, x => x.Description);
                derivedWageType.DescriptionLocalizations = CollectDerivedValue(derivedWageTypes, x => x.DescriptionLocalizations);
                derivedWageType.ValueType = CollectDerivedValue(derivedWageTypes, x => x.ValueType);
                derivedWageType.CalendarCalculationMode = CollectDerivedValue(derivedWageTypes, x => x.CalendarCalculationMode);
                derivedWageType.Collectors = CollectDerivedList(derivedWageTypes, x => x.Collectors);
                derivedWageType.CollectorGroups = CollectDerivedList(derivedWageTypes, x => x.CollectorGroups);
                derivedWageType.Attributes = CollectDerivedAttributes(derivedWageTypes);
                derivedWageType.Clusters = CollectDerivedList(derivedWageTypes, x => x.Clusters);
                // remove the current level for the next iteration
                derivedWageTypes.Remove(derivedWageType);
            }
        }
    }
}