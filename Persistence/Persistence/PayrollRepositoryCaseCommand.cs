using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Persistence.DbSchema;

namespace PayrollEngine.Persistence;

internal sealed class PayrollRepositoryCaseCommand : PayrollRepositoryCommandBase
{

    internal PayrollRepositoryCaseCommand(IDbContext dbContext) :
        base(dbContext)
    {
    }

    internal async Task<IEnumerable<DerivedCase>> GetDerivedCasesAsync(PayrollQuery query, CaseType? caseType = null,
        IEnumerable<string> caseNames = null, OverrideType? overrideType = null,
        ClusterSet clusterSet = null, bool? hidden = null)
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

        var names = caseNames?.Distinct().ToList();
        if (names != null)
        {
            foreach (var name in names)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException(nameof(caseNames));
                }
            }
        }

        // query setup
        query.RegulationDate ??= Date.Now;
        query.EvaluationDate ??= Date.Now;

        // parameters
        var parameters = new DbParameterCollection();
        parameters.Add(ParameterGetDerivedCases.TenantId, query.TenantId, DbType.Int32);
        parameters.Add(ParameterGetDerivedCases.PayrollId, query.PayrollId, DbType.Int32);
        parameters.Add(ParameterGetDerivedCases.CaseType,
            caseType, DbType.Int32);
        parameters.Add(ParameterGetDerivedCases.IncludeClusters,
            clusterSet?.IncludeClusters?.Any() == true
                ? JsonSerializer.Serialize(clusterSet.IncludeClusters) : null);
        parameters.Add(ParameterGetDerivedCases.ExcludeClusters,
            clusterSet?.ExcludeClusters?.Any() == true
                ? JsonSerializer.Serialize(clusterSet.ExcludeClusters) : null);
        parameters.Add(ParameterGetDerivedCases.CaseNames,
            names?.Any() == true ? JsonSerializer.Serialize(names) : null);
        parameters.Add(ParameterGetDerivedCases.Hidden,
            hidden, DbType.Boolean);
        parameters.Add(ParameterGetDerivedCases.RegulationDate, query.RegulationDate, DbType.DateTime2);
        parameters.Add(ParameterGetDerivedCases.CreatedBefore, query.EvaluationDate, DbType.DateTime2);

        // retrieve all derived cases (stored procedure)
        var cases = (await DbContext.QueryAsync<DerivedCase>(Procedures.GetDerivedCases,
            parameters, commandType: CommandType.StoredProcedure)).ToList();

        BuildDerivedCases(cases, overrideType);
        return cases;
    }

    private static void BuildDerivedCases(List<DerivedCase> cases, OverrideType? overrideType = null)
    {
        // argument check
        if (cases == null)
        {
            throw new ArgumentNullException(nameof(cases));
        }
        if (!cases.Any())
        {
            return;
        }

        // resulting cases
        var buildCases = new List<Case>(cases);
        var casesByKey = cases.GroupBy(x => x.Name).ToList();

        // override filter
        if (overrideType.HasValue)
        {
            ApplyOverrideFilter(casesByKey, cases, buildCases, overrideType.Value);
            // update cases
            casesByKey = cases.GroupBy(x => x.Name).ToList();
        }

        // collect derived values
        foreach (var @case in casesByKey)
        {
            // derived order
            var derivedCases = @case.OrderByDescending(x => x.Level).ThenByDescending(x => x.Priority).ToList();

            // derived cases
            while (derivedCases.Count > 1)
            {
                var derivedCase = derivedCases.First();
                // non-derived fields: case type, base case, base case fields, all non-nullable and expressions
                derivedCase.NameLocalizations = CollectDerivedValue(derivedCases, x => x.NameLocalizations);
                derivedCase.NameSynonyms = CollectDerivedList(derivedCases, x => x.NameSynonyms);
                derivedCase.Description = CollectDerivedValue(derivedCases, x => x.Description);
                derivedCase.DescriptionLocalizations = CollectDerivedValue(derivedCases, x => x.DescriptionLocalizations);
                derivedCase.DefaultReason = CollectDerivedValue(derivedCases, x => x.DefaultReason);
                derivedCase.DefaultReasonLocalizations = CollectDerivedValue(derivedCases, x => x.DefaultReasonLocalizations);
                derivedCase.Lookups = CollectDerivedList(derivedCases, x => x.Lookups);
                derivedCase.Slots = CollectDerivedList(derivedCases, x => x.Slots);
                derivedCase.AvailableActions = CollectDerivedList(derivedCases, x => x.AvailableActions);
                derivedCase.BuildActions = CollectDerivedList(derivedCases, x => x.BuildActions);
                derivedCase.ValidateActions = CollectDerivedList(derivedCases, x => x.ValidateActions);
                derivedCase.Attributes = CollectDerivedAttributes(derivedCases);
                derivedCase.Clusters = CollectDerivedList(derivedCases, x => x.Clusters);
                // remove the current level for the next iteration
                derivedCases.Remove(derivedCase);
            }
        }
    }
}