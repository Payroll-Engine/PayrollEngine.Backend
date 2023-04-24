using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence;

public class ScriptProviderRepository : RepositoryBase, IScriptProvider
{
    private static readonly Dictionary<Type, string> ScriptTables = new()
    {
        { typeof(Case), $"{nameof(Case)}"},
        { typeof(CaseSet), $"{nameof(Case)}"},
        { typeof(CaseRelation), $"{nameof(CaseRelation)}"},
        { typeof(Collector), $"{nameof(Collector)}"},
        { typeof(WageType), $"{nameof(WageType)}"},
        { typeof(Report), $"{nameof(Report)}"},
        { typeof(Payrun), $"{nameof(Payrun)}"}
    };

    public async Task<byte[]> GetBinaryAsync(IDbContext context, IScriptObject scriptObject)
    {
        // table name
        string tableName = null;
        var scriptType = scriptObject.GetType();
        foreach (var scriptTablesKey in ScriptTables.Keys)
        {
            if (scriptTablesKey.IsAssignableFrom(scriptType))
            {
                tableName = ScriptTables[scriptTablesKey];
                break;
            }
        }
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException($"Unsupported script type {scriptObject.GetType()}");
        }

        var query = DbQueryFactory.NewQuery(tableName)
            .Select(nameof(IScriptObject.Binary))
            .Where(nameof(IScriptObject.Id), scriptObject.Id)
            .Where(nameof(IScriptObject.ScriptHash), scriptObject.ScriptHash);
        var compileQuery = CompileQuery(query);

        var binary = await context.QueryFirstAsync<byte[]>(compileQuery);
        return binary;
    }
}