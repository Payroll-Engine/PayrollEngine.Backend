using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PayrollEngine.Api.Core;

public static class SwaggerApiKeySecurity
{
    public static void AddSwaggerApiKeySecurity(this SwaggerGenOptions options)
    {
        options.AddSecurityDefinition(BackendSpecification.ApiKeyHeader, new OpenApiSecurityScheme
        {
            Description = $"{BackendSpecification.ApiKeyHeader} must appear in header",
            Type = SecuritySchemeType.ApiKey,
            Name = BackendSpecification.ApiKeyHeader,
            In = ParameterLocation.Header,
            Scheme = "ApiKeyScheme"
        });

        // see https://www.reddit.com/r/dotnet/comments/1pd65xf/comment/ns2ogzf/?utm_source=share&utm_medium=web3x&utm_name=web3xcss&utm_term=1&utm_content=share_button
        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(BackendSpecification.ApiKeyHeader, document)] = []
        });
    }
}