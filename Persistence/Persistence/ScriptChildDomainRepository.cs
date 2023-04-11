using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Scripting;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Persistence;

public abstract class ScriptChildDomainRepository<TDomain, TScript> : ChildDomainRepository<TDomain>
    where TDomain : DomainObjectBase, new()
    where TScript : DomainObjectBase, new()
{
    public IScriptController<TScript> ScriptController { get; }
    // used in derived types to access the derived scripts
    public IScriptRepository ScriptRepository { get; }

    protected ScriptChildDomainRepository(string tableName, string parentFieldName,
        IScriptController<TScript> scriptController, IScriptRepository scriptRepository, IDbContext context) :
        base(tableName, parentFieldName, context)
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

    // duplicated in ScriptTrackChildDomainRepository!
    protected virtual async Task SetupBinaryAsync(int parentId, TDomain item)
    {
        if (item is not IScriptObject scriptObject)
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
        scriptObject.Script = result.Script;
        // set the current version
        scriptObject.ScriptVersion = ScriptingSpecification.ScriptingVersion.ToString();
        scriptObject.Binary = result.Binary;
        scriptObject.ScriptHash = result.Script.ToPayrollHash();
    }
}