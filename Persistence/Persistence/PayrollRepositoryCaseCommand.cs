﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence;

internal sealed class PayrollRepositoryCaseCommand : PayrollRepositoryCommandBase
{

    internal PayrollRepositoryCaseCommand(IDbContext dbContext) :
        base(dbContext)
    {
    }

    internal async Task<IEnumerable<Case>> GetDerivedCasesAsync(PayrollQuery query, CaseType? caseType = null,
        IEnumerable<string> caseNames = null, OverrideType? overrideType = null, ClusterSet clusterSet = null)
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
        parameters.Add(DbSchema.ParameterGetDerivedCases.TenantId, query.TenantId);
        parameters.Add(DbSchema.ParameterGetDerivedCases.PayrollId, query.PayrollId);
        if (caseType.HasValue)
        {
            parameters.Add(DbSchema.ParameterGetDerivedCases.CaseType, caseType.Value);
        }
        if (clusterSet != null)
        {
            if (clusterSet.IncludeClusters != null && clusterSet.IncludeClusters.Any())
            {
                parameters.Add(DbSchema.ParameterGetDerivedCases.IncludeClusters,
                    System.Text.Json.JsonSerializer.Serialize(clusterSet.IncludeClusters));
            }
            if (clusterSet.ExcludeClusters != null && clusterSet.ExcludeClusters.Any())
            {
                parameters.Add(DbSchema.ParameterGetDerivedCases.ExcludeClusters,
                    System.Text.Json.JsonSerializer.Serialize(clusterSet.ExcludeClusters));
            }
        }
        if (names != null && names.Any())
        {
            parameters.Add(DbSchema.ParameterGetDerivedCases.CaseNames,
                System.Text.Json.JsonSerializer.Serialize(names));
        }
        parameters.Add(DbSchema.ParameterGetDerivedCases.RegulationDate, query.RegulationDate);
        parameters.Add(DbSchema.ParameterGetDerivedCases.CreatedBefore, query.EvaluationDate);

        // retrieve all derived cases (stored procedure)
        var cases = (await DbContext.QueryAsync<DerivedCase>(DbSchema.Procedures.GetDerivedCases,
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
                derivedCase.NameLocalizations = CollectDerivedValue(derivedCases, x => x.NameLocalizations);
                derivedCase.Description = CollectDerivedValue(derivedCases, x => x.Description);
                derivedCase.DescriptionLocalizations = CollectDerivedValue(derivedCases, x => x.DescriptionLocalizations);
                derivedCase.DefaultReason = CollectDerivedValue(derivedCases, x => x.DefaultReason);
                derivedCase.DefaultReasonLocalizations = CollectDerivedValue(derivedCases, x => x.DefaultReasonLocalizations);
                derivedCase.CancellationType = CollectDerivedValue(derivedCases, x => x.CancellationType);
                derivedCase.NameSynonyms = CollectDerivedList(derivedCases, x => x.NameSynonyms);
                derivedCase.Lookups = CollectDerivedList(derivedCases, x => x.Lookups);
                derivedCase.Slots = CollectDerivedList(derivedCases, x => x.Slots);
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