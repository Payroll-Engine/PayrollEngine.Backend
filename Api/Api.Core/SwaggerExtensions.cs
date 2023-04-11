using Microsoft.AspNetCore.Builder;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace PayrollEngine.Api.Core;

public static class SwaggerExtensions
{
    public static IApplicationBuilder UseSwagger(this IApplicationBuilder appBuilder, 
        string apiDocumentationName, string apiName, string apiVersion)
    {
        appBuilder.UseSwagger();
        appBuilder.UseSwaggerUI(setupAction =>
        {
            setupAction.SetupSwaggerUI(apiDocumentationName, apiName, apiVersion);
            setupAction.DocExpansion(DocExpansion.None);

            // disable syntax highlight (performance issues on layout, e.g, get tenants)
            // https://stackoverflow.com/a/64696778 (exception on AdditionalItems field/property?)
            setupAction.ConfigObject.AdditionalItems.Add("syntaxHighlight", false);
        });
        return appBuilder;
    }
}