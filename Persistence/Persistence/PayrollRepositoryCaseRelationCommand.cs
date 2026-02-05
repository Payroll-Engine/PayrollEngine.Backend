using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence;

internal sealed class PayrollRepositoryCaseRelationCommand : PayrollRepositoryCommandBase
{
    internal PayrollRepositoryCaseRelationCommand(IDbContext dbContext) :
        base(dbContext)
    {
    }

    internal async Task<IEnumerable<DerivedCaseRelation>> GetDerivedCaseRelationsAsync(PayrollQuery query,
        string sourceCaseName = null, string targetCaseName = null,
        OverrideType? overrideType = null, ClusterSet clusterSet = null)
    {
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

        query.RegulationDate ??= Date.Now;
        query.EvaluationDate ??= Date.Now;
        // retrieve all derived case relations (stored procedure)
        var parameters = new DbParameterCollection();
        parameters.Add(DbSchema.ParameterGetDerivedCaseRelations.TenantId, query.TenantId, DbType.Int32);
        parameters.Add(DbSchema.ParameterGetDerivedCaseRelations.PayrollId, query.PayrollId, DbType.Int32);
        parameters.Add(DbSchema.ParameterGetDerivedCaseRelations.RegulationDate, query.RegulationDate, DbType.DateTime2);
        parameters.Add(DbSchema.ParameterGetDerivedCaseRelations.CreatedBefore, query.EvaluationDate, DbType.DateTime2);

        // source and target case
        if (sourceCaseName != null)
        {
            parameters.Add(DbSchema.ParameterGetDerivedCaseRelations.SourceCaseName, sourceCaseName);
        }
        if (targetCaseName != null)
        {
            parameters.Add(DbSchema.ParameterGetDerivedCaseRelations.TargetCaseName, targetCaseName);
        }

        // clusters
        if (clusterSet != null)
        {
            if (clusterSet.IncludeClusters != null && clusterSet.IncludeClusters.Any())
            {
                parameters.Add(DbSchema.ParameterGetDerivedCaseRelations.IncludeClusters,
                    System.Text.Json.JsonSerializer.Serialize(clusterSet.IncludeClusters));
            }
            if (clusterSet.ExcludeClusters != null && clusterSet.ExcludeClusters.Any())
            {
                parameters.Add(DbSchema.ParameterGetDerivedCaseRelations.ExcludeClusters,
                    System.Text.Json.JsonSerializer.Serialize(clusterSet.ExcludeClusters));
            }
        }

        // case relations
        var caseRelations = (await DbContext.QueryAsync<DerivedCaseRelation>(DbSchema.Procedures.GetDerivedCaseRelations,
            parameters, commandType: CommandType.StoredProcedure)).ToList();
        if (!caseRelations.Any())
        {
            return caseRelations;
        }

        BuildDerivedCaseRelations(caseRelations, overrideType);
        return caseRelations;
    }

    private static void BuildDerivedCaseRelations(List<DerivedCaseRelation> caseRelations, OverrideType? overrideType = null)
    {
        if (caseRelations == null)
        {
            throw new ArgumentNullException(nameof(caseRelations));
        }
        if (!caseRelations.Any())
        {
            return;
        }

        // case relations
        var caseRelationsByKey = caseRelations.GroupBy(x => new { x.TargetCaseName, x.SourceCaseName, x.SourceCaseSlot, x.TargetCaseSlot }).ToList();

        // override filter
        if (overrideType.HasValue)
        {
            ApplyOverrideFilter(caseRelationsByKey, caseRelations, overrideType.Value);
            // update case relations
            caseRelationsByKey = caseRelations.GroupBy(x => new { x.TargetCaseName, x.SourceCaseName, x.SourceCaseSlot, x.TargetCaseSlot }).ToList();
        }

        // collect derived values
        foreach (var caseRelation in caseRelationsByKey)
        {
            var derivedCaseRelations = caseRelation.OrderByDescending(x => x.Level).ThenByDescending(x => x.Priority).ToList();
            // derived case relations
            while (derivedCaseRelations.Count > 1)
            {
                var derivedCaseRelation = derivedCaseRelations.First();
                // non-derived fields: source/target case name/slot and all non-nullable and expressions
                derivedCaseRelation.SourceCaseNameLocalizations = CollectDerivedValue(derivedCaseRelations, x => x.SourceCaseNameLocalizations);
                derivedCaseRelation.SourceCaseSlotLocalizations = CollectDerivedValue(derivedCaseRelations, x => x.SourceCaseSlotLocalizations);
                derivedCaseRelation.TargetCaseNameLocalizations = CollectDerivedValue(derivedCaseRelations, x => x.TargetCaseNameLocalizations);
                derivedCaseRelation.TargetCaseSlotLocalizations = CollectDerivedValue(derivedCaseRelations, x => x.TargetCaseSlotLocalizations);
                derivedCaseRelation.BuildActions = CollectDerivedList(derivedCaseRelations, x => x.BuildActions);
                derivedCaseRelation.ValidateActions = CollectDerivedList(derivedCaseRelations, x => x.ValidateActions);
                derivedCaseRelation.Attributes = CollectDerivedAttributes(derivedCaseRelations);
                derivedCaseRelation.Clusters = CollectDerivedList(derivedCaseRelations, x => x.Clusters);
                // remove the current level for the next iteration
                derivedCaseRelations.Remove(derivedCaseRelation);
            }
        }
    }
}