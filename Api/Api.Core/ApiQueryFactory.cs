using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace PayrollEngine.Api.Core;

public static class ApiQueryFactory
{
    /// <summary>
    /// Get query method names
    /// </summary>
    /// <returns>The query method names</returns>
    public static List<string> GetQueryNames() =>
        FromEntryAssembly().Keys.OrderBy(x => x).ToList();

    /// <summary>
    /// Loads queries on demand
    /// </summary>
    internal static Dictionary<string, QueryMethodInfo> FromEntryAssembly()
    {
        var queries = new Dictionary<string, QueryMethodInfo>();

        // query methods
        var assembly = Assembly.GetEntryAssembly();
        if (assembly == null)
        {
            throw new QueryException("Missing startup assembly");
        }

        // extract all query methods
        var queryTypes = assembly.GetTypes();
        foreach (var queryType in queryTypes)
        {
            var queryMethods = queryType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                .Where(m => m.GetCustomAttributes(typeof(HttpGetAttribute), true).Any());
            foreach (var queryMethod in queryMethods)
            {
                // attribute ApiOperationId is mandatory
                var operationId = queryMethod.GetCustomAttributes(typeof(ApiOperationIdAttribute), false)
                    .FirstOrDefault() as ApiOperationIdAttribute;
                if (string.IsNullOrWhiteSpace(operationId?.OperationId))
                {
                    continue;
                }

                // query ignore
                if (queryMethod.GetCustomAttributes(typeof(QueryIgnoreAttribute), false).Any())
                {
                    continue;
                }

                queries.Add(operationId.OperationId, new(queryType, queryMethod));
            }
        }

        return queries;
    }

}