using Microsoft.AspNetCore.Builder;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace PayrollEngine.Api.Core;

public static class SwaggerExtensions
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IApplicationBuilder UseSwagger(this IApplicationBuilder appBuilder,
        string apiDocumentationName, string apiName, string apiVersion, bool darkTheme)
    {
        appBuilder.UseSwagger();
        appBuilder.UseSwaggerUI(setupAction =>
        {
            setupAction.SetupSwaggerUI(apiDocumentationName, apiName, apiVersion);
            setupAction.DocExpansion(DocExpansion.None);

            // disable syntax highlight (performance issues on layout, e.g, get tenants)
            // https://stackoverflow.com/a/64696778 (exception on AdditionalItems field/property?)
            setupAction.ConfigObject.AdditionalItems.Add("syntaxHighlight", false);

            // custom css
            setupAction.InjectStylesheet("/swagger-ui/Swagger.css");

            // dark theme css
            if (darkTheme)
            {
                setupAction.InjectStylesheet("/swagger-ui/SwaggerDark.css");
            }
        });
        return appBuilder;
    }
}