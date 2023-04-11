using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence;

internal sealed class PayrollRepositoryCollectorCommand : PayrollRepositoryCommandBase
{
    /// <summary>Derived collector</summary>
    private sealed class DerivedCollector : Collector
    {
        /// <summary>The layer level</summary>
        internal int Level { get; private set; }

        /// <summary>The layer priority</summary>
        internal int Priority { get; private set; }
    }

    internal PayrollRepositoryCollectorCommand(IDbConnection connection) :
        base(connection)
    {
    }

    internal async Task<IEnumerable<Collector>> GetDerivedCollectorsAsync(PayrollQuery query,
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
        parameters.Add(DbSchema.ParameterGetDerivedCollectors.TenantId, query.TenantId);
        parameters.Add(DbSchema.ParameterGetDerivedCollectors.PayrollId, query.PayrollId);
        parameters.Add(DbSchema.ParameterGetDerivedCollectors.RegulationDate, query.RegulationDate);
        parameters.Add(DbSchema.ParameterGetDerivedCollectors.CreatedBefore, query.EvaluationDate);
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
        var collectors = (await Connection.QueryAsync<DerivedCollector>(DbSchema.Procedures.GetDerivedCollectors,
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
                derivedCollector.NameLocalizations = CollectDerivedValue(derivedCollectors, x => x.NameLocalizations);
                derivedCollector.CollectType = CollectDerivedValue(derivedCollectors, x => x.CollectType);
                derivedCollector.ValueType = CollectDerivedValue(derivedCollectors, x => x.ValueType);
                derivedCollector.CollectorGroups = CollectDerivedList(derivedCollectors, x => x.CollectorGroups);
                derivedCollector.Attributes = CollectDerivedAttributes(derivedCollectors);
                derivedCollector.Clusters = CollectDerivedList(derivedCollectors, x => x.Clusters);
                // remove the current level for the next iteration
                derivedCollectors.Remove(derivedCollector);
            }
        }
    }
}