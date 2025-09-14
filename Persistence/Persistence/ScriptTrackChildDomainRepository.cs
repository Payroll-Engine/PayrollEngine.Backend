using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Scripting;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Persistence;

public abstract class ScriptTrackChildDomainRepository<TDomain, TAudit>(string tableName, string parentFieldName,
        IScriptRepository scriptRepository, IAuditChildDomainRepository<TAudit> auditRepository, bool auditDisabled)
    : TrackChildDomainRepository<TDomain, TAudit>(tableName, parentFieldName, auditRepository, auditDisabled),
        IScriptTrackDomainObjectRepository<TDomain, TAudit>
    where TDomain : TrackDomainObject<TAudit>, new()
    where TAudit : AuditDomainObject
{
    // used in derived types to access the derived scripts
    private IScriptRepository ScriptRepository { get; } = scriptRepository ?? throw new ArgumentNullException(nameof(scriptRepository));

    public override async Task<TDomain> CreateAsync(IDbContext context, int parentId, TDomain item)
    {
        await SetupBinaryAsync(context, parentId, item);
        return await base.CreateAsync(context, parentId, item);
    }

    public override async Task<TDomain> UpdateAsync(IDbContext context, int parentId, TDomain item)
    {
        await SetupBinaryAsync(context, parentId, item);
        return await base.UpdateAsync(context, parentId, item);
    }

    // duplicated in Payrun!
    public virtual async Task RebuildAsync(IDbContext context, int parentId, int itemId)
    {
        if (parentId == 0)
        {
            throw new ArgumentException(nameof(parentId));
        }
        if (itemId == 0)
        {
            throw new ArgumentNullException(nameof(itemId));
        }

        // create transaction
        using var txScope = TransactionFactory.NewTransactionScope();

        // read item
        var item = await GetAsync(context, parentId, itemId);
        if (item == null)
        {
            throw new PayrollException($"Unknown script object {typeof(TDomain)} with id {itemId}.");
        }
        var scriptObject = item as IScriptObject;
        if (scriptObject == null)
        {
            throw new PayrollException($"Invalid script object {typeof(TDomain)} with id {itemId}.");
        }

        // rebuild script binary
        await SetupBinaryAsync(context, parentId, item);

        // update item
        await UpdateAsync(context, parentId, item);

        // commit transaction
        txScope.Complete();
    }

    // duplicated in ScriptChildDomainSqlRepository!
    private async Task SetupBinaryAsync(IDbContext context, int parentId, TDomain item)
    {
        if (!(item is IScriptObject scriptObject))
        {
            return;
        }

        if (!scriptObject.HasExpression)
        {
            scriptObject.Clear();
            return;
        }

        // used object functions
        var functionScripts = scriptObject.GetFunctionScripts();

        // object scripts (optional)
        IEnumerable<Script> scripts = null;
        if (functionScripts.Any() && scriptObject.HasObjectScripts)
        {
            scripts = await ScriptRepository.GetFunctionScriptsAsync(context, parentId, functionScripts.Keys.ToList());
        }

        // embedded scripts (optional)
        var embeddedScripts = scriptObject.GetEmbeddedScriptNames();

        // compilation
        var result = new ScriptCompiler(item, functionScripts, scripts, embeddedScripts).Compile();

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