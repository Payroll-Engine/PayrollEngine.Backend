using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Persistence.DbSchema;

namespace PayrollEngine.Persistence;

internal sealed class PayrollRepositoryScriptCommand : PayrollRepositoryCommandBase
{
    internal PayrollRepositoryScriptCommand(IDbContext dbContext) :
        base(dbContext)
    {
    }

    /// <summary>
    /// Get derived payroll scripts
    /// </summary>
    /// <param name="scriptRepository">The script repository</param>
    /// <param name="query">The query</param>
    /// <param name="scriptNames">The script names</param>
    /// <param name="overrideType">The override type</param>
    /// <returns>The derived scripts, ordered by derivation level</returns>
    internal async Task<IEnumerable<DerivedScript>> GetDerivedScriptsAsync(
        IScriptRepository scriptRepository,
        PayrollQuery query, IEnumerable<string> scriptNames = null,
        OverrideType? overrideType = null)
    {
        // argument check
        if (scriptRepository == null)
        {
            throw new ArgumentNullException(nameof(scriptRepository));
        }
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
        var names = scriptNames?.Distinct().ToList();
        if (names != null)
        {
            foreach (var name in names)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException(nameof(scriptNames));
                }
            }
        }

        // query setup
        query.RegulationDate ??= Date.Now;
        query.EvaluationDate ??= Date.Now;

        // parameters
        var parameters = new DbParameterCollection();
        parameters.Add(ParameterGetDerivedScripts.TenantId, query.TenantId, DbType.Int32);
        parameters.Add(ParameterGetDerivedScripts.PayrollId, query.PayrollId, DbType.Int32);
        parameters.Add(ParameterGetDerivedScripts.RegulationDate, query.RegulationDate, DbType.DateTime2);
        parameters.Add(ParameterGetDerivedScripts.CreatedBefore, query.EvaluationDate, DbType.DateTime2);
        parameters.Add(ParameterGetDerivedScripts.ScriptNames,
            names?.Any() == true ? JsonSerializer.Serialize(names) : null);

        // retrieve all derived scripts (stored procedure)
        var scripts = (await DbContext.QueryAsync<DerivedScript>(Procedures.GetDerivedScripts,
            parameters, commandType: CommandType.StoredProcedure)).ToList();

        BuildDerivedScripts(scripts, overrideType);

        // build script sets
        var scriptSets = new List<DerivedScript>();
        foreach (var script in scripts)
        {
            // load script content manually
            var scriptSet = await scriptRepository.GetAsync(DbContext, script.RegulationId, script.Id);
            var derivedScriptSte = new DerivedScript(scriptSet)
            {
                RegulationId = script.RegulationId,
                Level = script.Level,
                Priority = script.Priority
            };
            scriptSets.Add(derivedScriptSte);
        }

        return scriptSets;
    }

    private static void BuildDerivedScripts(List<DerivedScript> scripts, OverrideType? overrideType = null)
    {
        if (scripts == null)
        {
            throw new ArgumentNullException(nameof(scripts));
        }
        if (!scripts.Any())
        {
            return;
        }

        // resulting scripts
        var scriptsByKey = scripts.GroupBy(x => x.Name).ToList();

        // override filter
        if (overrideType.HasValue)
        {
            ApplyOverrideFilter(scriptsByKey, scripts, overrideType.Value);
            // update scripts
            scriptsByKey = scripts.GroupBy(x => x.Name).ToList();
        }

        // collect derived values
        foreach (var scriptKey in scriptsByKey)
        {
            // order by derived scripts
            var derivedScripts = scriptKey.OrderByDescending(x => x.Level).ThenByDescending(x => x.Priority).ToList();
            // derived scripts
            while (derivedScripts.Count > 1)
            {
                // collect derived values
                var derivedScript = derivedScripts.First();
                derivedScript.Value = CollectDerivedValue(derivedScripts, x => x.Value);
                // remove the current level for the next iteration
                derivedScripts.Remove(derivedScript);
            }
        }
    }
}