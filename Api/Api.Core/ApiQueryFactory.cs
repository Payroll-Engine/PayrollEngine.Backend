using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace PayrollEngine.Api.Core;

public static class ApiQueryFactory
{
    /// <summary>Get query method names (GET endpoints)</summary>
    /// <returns>The query method names</returns>
    public static List<string> GetQueryNames() =>
        GetQueryMethods().Keys.OrderBy(x => x).ToList();

    /// <summary>Loads queries on demand (GET endpoints)</summary>
    internal static Dictionary<string, QueryMethodInfo> GetQueryMethods()
    {
        var queries = new Dictionary<string, QueryMethodInfo>();

        // available assemblies, excluding system and known assemblies
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(x => x.FullName != null &&
                        !x.FullName.StartsWith("System") &&
                        !x.FullName.StartsWith("Microsoft") &&
                        !x.FullName.StartsWith("Serilog") &&
                        !x.FullName.StartsWith("NLog") &&
                        !x.FullName.StartsWith("Swashbuckle"));

        // collect query methods from any assemblies, marked with 'query assembly attribute'
        var assemblyCount = 0;
        foreach (var assembly in assemblies)
        {
            // assembly filter
            var queryAssembly = assembly.GetCustomAttributes(typeof(QueryAssemblyAttribute), false)
                .FirstOrDefault() as QueryAssemblyAttribute;
            if (queryAssembly == null)
            {
                continue;
            }
            assemblyCount++;

            // extract all query methods, excluding methods which are marked with the 'query ignore attribute'
            var queryTypes = assembly.GetTypes();
            foreach (var queryType in queryTypes)
            {
                // process all GET endpoints
                var queryMethods = queryType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                    .Where(m => m.GetCustomAttributes(typeof(HttpGetAttribute), true).Any()).ToList();
                foreach (var queryMethod in queryMethods)
                {
                    // ensure mandatory attribute 'api operation id'
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
        }

        // missing queries
        if (!queries.Any())
        {
            Log.Warning(assemblyCount == 0
                ? "Missing API query assembly: use the QueryAssemblyAttribute to mark the assembly with the controller endpoints"
                : "No API query methods available");
        }

        return queries;
    }

}