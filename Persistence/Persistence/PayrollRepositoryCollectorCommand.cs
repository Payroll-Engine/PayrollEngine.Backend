using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence;

internal sealed class PayrollRepositoryCollectorCommand : PayrollRepositoryCommandBase
{
    internal PayrollRepositoryCollectorCommand(IDbContext dbContext) :
        base(dbContext)
    {
    }

    internal async Task<IEnumerable<DerivedCollector>> GetDerivedCollectorsAsync(PayrollQuery query,
        IEnumerable<string> collectorNames = null,
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
        var names = collectorNames?.Distinct().ToList();
        if (names != null)
        {
            foreach (var name in names)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException(nameof(collectorNames));
                }
            }
        }

        // query setup
        query.RegulationDate ??= Date.Now;
        query.EvaluationDate ??= Date.Now;

        // parameters
        var parameters = new DbParameterCollection();
        parameters.Add(DbSchema.ParameterGetDerivedCollectors.TenantId, query.TenantId, DbType.Int32);
        parameters.Add(DbSchema.ParameterGetDerivedCollectors.PayrollId, query.PayrollId, DbType.Int32);
        parameters.Add(DbSchema.ParameterGetDerivedCollectors.RegulationDate, query.RegulationDate, DbType.DateTime2);
        parameters.Add(DbSchema.ParameterGetDerivedCollectors.CreatedBefore, query.EvaluationDate, DbType.DateTime2);
        if (clusterSet != null)
        {
            if (clusterSet.IncludeClusters != null && clusterSet.IncludeClusters.Any())
            {
                parameters.Add(DbSchema.ParameterGetDerivedCollectors.IncludeClusters,
                    System.Text.Json.JsonSerializer.Serialize(clusterSet.IncludeClusters));
            }
            if (clusterSet.ExcludeClusters != null && clusterSet.ExcludeClusters.Any())
            {
                parameters.Add(DbSchema.ParameterGetDerivedCollectors.ExcludeClusters,
                    System.Text.Json.JsonSerializer.Serialize(clusterSet.ExcludeClusters));
            }
        }
        if (names != null && names.Any())
        {
            parameters.Add(DbSchema.ParameterGetDerivedCollectors.CollectorNames,
                System.Text.Json.JsonSerializer.Serialize(names));
        }

        // retrieve all derived collectors (stored procedure)
        var collectors = (await DbContext.QueryAsync<DerivedCollector>(DbSchema.Procedures.GetDerivedCollectors,
            parameters, commandType: CommandType.StoredProcedure)).ToList();

        BuildDerivedCollectors(collectors, overrideType);
        return collectors;
    }

    private static void BuildDerivedCollectors(List<DerivedCollector> collectors, OverrideType? overrideType = null)
    {
        if (collectors == null)
        {
            throw new ArgumentNullException(nameof(collectors));
        }
        if (!collectors.Any())
        {
            return;
        }

        // collectors
        var collectorsByKey = collectors.GroupBy(x => x.Name).ToList();

        // override filter
        if (overrideType.HasValue)
        {
            ApplyOverrideFilter(collectorsByKey, collectors, overrideType.Value);
            // updated collectors
            collectorsByKey = collectors.GroupBy(x => x.Name).ToList();
        }

        // collect derived values
        foreach (var collector in collectorsByKey)
        {
            // derived order
            var derivedCollectors = collector.OrderByDescending(x => x.Level).ThenByDescending(x => x.Priority).ToList();
            // derived collector
            while (derivedCollectors.Count > 1)
            {
                var derivedCollector = derivedCollectors.First();
                // non-derived fields: name, collect mode. negated and all non-nullable and expressions
                derivedCollector.NameLocalizations = CollectDerivedValue(derivedCollectors, x => x.NameLocalizations);
                derivedCollector.CollectorGroups = CollectDerivedList(derivedCollectors, x => x.CollectorGroups);
                derivedCollector.Threshold = CollectDerivedValue(derivedCollectors, x => x.Threshold);
                derivedCollector.MinResult = CollectDerivedValue(derivedCollectors, x => x.MinResult);
                derivedCollector.MaxResult = CollectDerivedValue(derivedCollectors, x => x.MaxResult);
                derivedCollector.StartActions = CollectDerivedList(derivedCollectors, x => x.StartActions);
                derivedCollector.ApplyActions = CollectDerivedList(derivedCollectors, x => x.ApplyActions);
                derivedCollector.EndActions = CollectDerivedList(derivedCollectors, x => x.EndActions);
                derivedCollector.Attributes = CollectDerivedAttributes(derivedCollectors);
                derivedCollector.Clusters = CollectDerivedList(derivedCollectors, x => x.Clusters);
                // remove the current level for the next iteration
                derivedCollectors.Remove(derivedCollector);
            }
        }
    }
}