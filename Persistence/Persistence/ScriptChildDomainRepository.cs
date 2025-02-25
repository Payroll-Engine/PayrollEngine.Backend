using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Scripting;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Persistence;

public abstract class ScriptChildDomainRepository<TDomain>(string tableName, string parentFieldName,
        IScriptRepository scriptRepository)
    : ChildDomainRepository<TDomain>(tableName, parentFieldName)
    where TDomain : DomainObjectBase, new()
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

    // duplicated in ScriptTrackChildDomainRepository!
    protected async Task SetupBinaryAsync(IDbContext context, int parentId, TDomain item)
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
        scriptObject.Script = result.Script;
        // set the current version
        scriptObject.ScriptVersion = ScriptingSpecification.ScriptingVersion.ToString();
        scriptObject.Binary = result.Binary;
        scriptObject.ScriptHash = result.Script.ToPayrollHash();
    }
}