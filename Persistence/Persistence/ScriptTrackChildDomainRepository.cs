using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Task = System.Threading.Tasks.Task;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Scripting;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public abstract class ScriptTrackChildDomainRepository<TDomain, TAudit>(string tableName,
        string regulationFieldName, IRegulationRepository regulationRepository,
        IScriptRepository scriptRepository, IAuditChildDomainRepository<TAudit> auditRepository, bool auditDisabled)
    : TrackChildDomainRepository<TDomain, TAudit>(regulationRepository, tableName, regulationFieldName, 
            auditRepository, auditDisabled), IScriptTrackDomainObjectRepository<TDomain, TAudit>
    where TDomain : TrackDomainObject<TAudit>, IScriptObject, INamespaceObject, new()
    where TAudit : AuditDomainObject
{
    // used in derived types to access the derived scripts
    private IScriptRepository ScriptRepository { get; } = scriptRepository ?? throw new ArgumentNullException(nameof(scriptRepository));

    public override async Task<TDomain> CreateAsync(IDbContext context, int regulationId, TDomain item)
    {
        // namespace
        await ApplyNamespaceAsync(context, regulationId, item);
        await SetupBinaryAsync(context, regulationId, item);
        return await base.CreateAsync(context, regulationId, item);
    }

    public override async Task<TDomain> UpdateAsync(IDbContext context, int regulationId, TDomain item)
    {
        // namespace
        await ApplyNamespaceAsync(context, regulationId, item);
        await SetupBinaryAsync(context, regulationId, item);
        return await base.UpdateAsync(context, regulationId, item);
    }

    // variation in Payrun!
    public virtual async Task RebuildAsync(IDbContext context, int regulationId, int itemId)
    {
        if (regulationId == 0)
        {
            throw new ArgumentException(nameof(regulationId));
        }
        if (itemId == 0)
        {
            throw new ArgumentNullException(nameof(itemId));
        }

        // create transaction
        using var txScope = TransactionFactory.NewTransactionScope();

        // read item
        var item = await GetAsync(context, regulationId, itemId);
        if (item == null)
        {
            throw new PayrollException($"Unknown script object {typeof(TDomain)} with id {itemId}.");
        }

        // rebuild script binary
        await SetupBinaryAsync(context, regulationId, item);

        // update item
        await UpdateAsync(context, regulationId, item);

        // commit transaction
        txScope.Complete();
    }

    // variation in ScriptChildDomainRepository!
    private async Task SetupBinaryAsync(IDbContext context, int regulationId, TDomain item)
    {
        if (!(item is IScriptObject scriptObject))
        {
            return;
        }

        if (!scriptObject.HasAnyExpression)
        {
            scriptObject.Clear();
            return;
        }

        // collect function scripts
        var functionScripts = new Dictionary<FunctionType, string>();
        foreach (var functionType in scriptObject.GetFunctionTypes())
        {
            var script = scriptObject.GetFunctionScript(functionType);
            var actions = scriptObject.GetFunctionActions(functionType);
            if (string.IsNullOrWhiteSpace(script) &&
                (actions == null || !actions.Any()))
            {
                continue;
            }
            functionScripts[functionType] = script;
        }

        // object scripts (optional)
        IEnumerable<Script> scripts = null;
        if (functionScripts.Any() && scriptObject.HasObjectScripts)
        {
            scripts = await ScriptRepository.GetFunctionScriptsAsync(
                context: context,
                regulationId: regulationId,
                functionTypes: functionScripts.Keys.ToList());
        }

        // embedded scripts (optional)
        var embeddedScriptNames = scriptObject.GetEmbeddedScriptNames();

        // compilation
        var @namespace = await GetRegulationNamespaceAsync(context, regulationId);
        var result = new ScriptCompiler(
            scriptObject: scriptObject,
            functionScripts: functionScripts,
            scripts: scripts,
            embeddedScriptNames: embeddedScriptNames,
            @namespace: @namespace).Compile();

        // result
        if (result == null)
        {
            scriptObject.Clear();
            return;
        }

#if DEBUG
        // performance optimization: added script source only in debug mode
        scriptObject.Script = result.Script;
#endif

        // set the current version
        scriptObject.ScriptVersion = ScriptingSpecification.ScriptingVersion.ToString();
        scriptObject.Binary = result.Binary;
        scriptObject.ScriptHash = result.Script.ToPayrollHash();
    }
}