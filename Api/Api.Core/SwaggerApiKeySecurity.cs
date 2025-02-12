using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.DependencyInjection;
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
        var key = new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = BackendSpecification.ApiKeyHeader
            },
            In = ParameterLocation.Header
        };
        var requirement = new OpenApiSecurityRequirement { { key, new List<string>() } };
        options.AddSecurityRequirement(requirement);
    }
}