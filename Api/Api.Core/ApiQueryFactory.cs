using System;
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

        var allAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(x => x.FullName != null &&
                        !x.FullName.StartsWith("System") &&
                        !x.FullName.StartsWith("Microsoft") &&
                        !x.FullName.StartsWith("Serilog") &&
                        !x.FullName.StartsWith("NLog") &&
                        !x.FullName.StartsWith("Swashbuckle"))
            .ToList();
        foreach (var assembly in allAssemblies)
        {
            // extract all query methods
            var queryTypes = assembly.GetTypes();
            foreach (var queryType in queryTypes)
            {
                var queryMethods = queryType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                    .Where(m => m.GetCustomAttributes(typeof(HttpGetAttribute), true).Any()).ToList();
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
        }

        return queries;
    }

}