using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PayrollEngine.Api.Core;

/// <summary>
/// Sort operations by key
/// Please note: the operation id needs to be unique over all operations
/// </summary>
public class SwaggerDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        if (swaggerDoc == null)
        {
            return;
        }

        OrderOperationParameters(swaggerDoc);
        OrderSchemas(swaggerDoc);
    }

    private static void OrderOperationParameters(OpenApiDocument swaggerDoc)
    {
        foreach (var path in swaggerDoc.Paths)
        {
            foreach (var operation in path.Value.Operations)
            {
                if (operation.Value.Parameters.Count <= 1)
                {
                    continue;
                }

                var parameters = new List<OpenApiParameter>();

                // priority 1: path parameters
                AppendParameters(parameters, operation.Value.Parameters
                    .Where(p => p.In == ParameterLocation.Path));

                // priority 2: required query parameters
                AppendParameters(parameters, operation.Value.Parameters
                    .Where(p => p.Required &&
                                p.In != ParameterLocation.Path));

                // priority 3: optional query id parameters sorted by name
                AppendParameters(parameters, operation.Value.Parameters
                    .Where(p => !p.Required &&
                                p.In != ParameterLocation.Path &&
                                p.Name.EndsWith("Id"))
                    .OrderBy(x => x.Name));

                // priority 4: optional query non-id parameters sorted by name
                AppendParameters(parameters, operation.Value.Parameters
                    .Where(p => !p.Required &&
                                p.In != ParameterLocation.Path &&
                                !p.Name.EndsWith("Id"))
                    .OrderBy(x => x.Name));

                operation.Value.Parameters = parameters;
            }
        }
    }

    private static void AppendParameters(List<OpenApiParameter> parameters, IEnumerable<OpenApiParameter> items)
    {
        foreach (var item in items)
        {
            if (parameters.All(x => !string.Equals(x.Name, item.Name, StringComparison.InvariantCultureIgnoreCase)))
            {
                parameters.Add(item);
            }
        }
    }

    private static void OrderSchemas(OpenApiDocument swaggerDoc)
    {
        // reorder schemas alphabetically
        swaggerDoc.Components.Schemas = swaggerDoc.Components.Schemas
            .OrderBy(kvp => kvp.Key, StringComparer.InvariantCulture)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
}