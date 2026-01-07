using System.Linq;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Mvc.Controllers;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PayrollEngine.Api.Core;

/// <summary>
/// Apply a custom operation id to the OpenAPI operation
/// Please note: the operation id needs to be unique over all operations
/// </summary>
public class SwaggerOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.ApiDescription.ActionDescriptor is ControllerActionDescriptor descriptor)
        {
            if (descriptor.MethodInfo.GetCustomAttributes(typeof(ApiOperationIdAttribute), true).FirstOrDefault() is ApiOperationIdAttribute attribute)
            {
                operation.OperationId = attribute.OperationId;
            }
        }
    }
}