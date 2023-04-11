using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Scripting;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Persistence;

public abstract class ScriptTrackChildDomainRepository<TDomain, TAudit> : TrackChildDomainRepository<TDomain, TAudit>,
    IScriptTrackDomainObjectRepository<TDomain, TAudit>
    where TDomain : TrackDomainObject<TAudit>, new()
    where TAudit : AuditDomainObject
{
    public IScriptController<TDomain> ScriptController { get; }
    // used in derived types to access the derived scripts
    public IScriptRepository ScriptRepository { get; }

    protected ScriptTrackChildDomainRepository(string tableName, string parentFieldName,
        IScriptController<TDomain> scriptController, IScriptRepository scriptRepository,
        IAuditChildDomainRepository<TAudit> auditRepository, IDbContext context) :
        base(tableName, parentFieldName, auditRepository, context)
    {
        ScriptController = scriptController ?? throw new ArgumentNullException(nameof(scriptController));
        ScriptRepository = scriptRepository ?? throw new ArgumentNullException(nameof(scriptRepository));
    }

    public override async Task<TDomain> CreateAsync(int parentId, TDomain item)
    {
        await SetupBinaryAsync(parentId, item);
        return await base.CreateAsync(parentId, item);
    }

    public override async Task<TDomain> UpdateAsync(int parentId, TDomain item)
    {
        await SetupBinaryAsync(parentId, item);
        return await base.UpdateAsync(parentId, item);
    }

    // duplicated in Payrun!
    public virtual async Task RebuildAsync(int parentId, int itemId)
    {
        if (parentId == default)
        {
            throw new ArgumentException(nameof(parentId));
        }
        if (itemId == default)
        {
            throw new ArgumentNullException(nameof(itemId));
        }

        // create transaction
        using var txScope = TransactionFactory.NewTransactionScope();

        // read item
        var item = await GetAsync(parentId, itemId);
        if (item == null)
        {
            throw new PayrollException($"Unknown script object {typeof(TDomain)} with id {itemId}");
        }
        var scriptObject = item as IScriptObject;
        if (scriptObject == null)
        {
            throw new PayrollException($"Invalid script object {typeof(TDomain)} with id {itemId}");
        }

        // rebuild script binary
        await SetupBinaryAsync(parentId, item);

        // update item
        await UpdateAsync(parentId, item);

        // commit transaction
        txScope.Complete();
    }

    // duplicated in ScriptChildDomainSqlRepository!
    protected virtual async Task SetupBinaryAsync(int parentId, TDomain item)
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
            scripts = await ScriptRepository.GetFunctionScriptsAsync(parentId, functionScripts.Keys.ToList());
        }

        // embedded scripts (optional)
        var embeddedScripts = scriptObject.GetEmbeddedScriptNames();

        // compilation
        var result = new ScriptCompiler(typeof(TDomain), functionScripts, scripts, embeddedScripts).Compile();

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