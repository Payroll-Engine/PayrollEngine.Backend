using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class ScriptRepository(IRegulationRepository regulationRepository, IScriptAuditRepository auditRepository, 
    bool auditDisabled) : TrackChildDomainRepository<Script, ScriptAudit>(regulationRepository,
    DbSchema.Tables.Script, DbSchema.ScriptColumn.RegulationId, auditRepository, auditDisabled), IScriptRepository
{
    protected override void GetObjectCreateData(Script script, DbParameterCollection parameters)
    {
        parameters.Add(nameof(script.Name), script.Name);
        base.GetObjectCreateData(script, parameters);
    }

    protected override void GetObjectData(Script script, DbParameterCollection parameters)
    {
        parameters.Add(nameof(script.FunctionTypeMask), script.FunctionTypeMask, DbType.Int64);
        parameters.Add(nameof(script.Value), script.Value);
        parameters.Add(nameof(script.OverrideType), script.OverrideType, DbType.Int32);
        base.GetObjectData(script, parameters);
    }

    public async Task<bool> ExistsAnyAsync(IDbContext context, int regulationId, IEnumerable<string> scriptNames) =>
        await ExistsAnyAsync(context, DbSchema.ScriptColumn.RegulationId, regulationId, DbSchema.ScriptColumn.Name, scriptNames);

    public async Task<IEnumerable<Script>> GetFunctionScriptsAsync(IDbContext context, int regulationId,
        List<FunctionType> functionTypes = null, DateTime? evaluationDate = null)
    {
        if (regulationId <= 0)
        {
            throw new ArgumentException(nameof(regulationId));
        }

        // query
        var query = DbQueryFactory.NewQuery(TableName, ParentFieldName, regulationId);
        // ignore newer created objects
        if (evaluationDate.HasValue)
        {
            query.Where(DbSchema.ObjectColumn.Created, "<", evaluationDate);
        }

        // order from newest to oldest
        var compileQuery = CompileQuery(query);

        // filter by function types
        if (functionTypes != null)
        {
            var bitmask = functionTypes.ToBitmask();
            // Dapper does not support where clause with bitwise AND
            // see also https://docs.microsoft.com/en-us/sql/t-sql/language-elements/bitwise-and-transact-sql
            compileQuery += $" AND ([{DbSchema.ScriptColumn.FunctionTypeMask}] & {bitmask} <> 0 OR [{DbSchema.ScriptColumn.FunctionTypeMask}] = 0)";
        }

        var scripts = (await QueryAsync<Script>(context, compileQuery)).ToList();

        // notifications
        await OnRetrieved(context, regulationId, scripts);

        return scripts;
    }

    public override async Task<Script> CreateAsync(IDbContext context, int regulationId, Script script)
    {
        await ApplyNamespaceAsync(context, regulationId, script);
        return await base.CreateAsync(context, regulationId, script);
    }

    public override async Task<Script> UpdateAsync(IDbContext context, int regulationId, Script script)
    {
        await ApplyNamespaceAsync(context, regulationId, script);
        return await base.UpdateAsync(context, regulationId, script);
    }
}