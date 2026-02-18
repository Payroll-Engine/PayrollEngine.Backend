using System;
using System.Collections.Generic;
using Microsoft.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PayrollEngine.Api.Core;

/// <summary>
/// Swagger OAuth security
/// </summary>
public static class SwaggerOAuthSecurity
{
    /// <summary>
    /// Add OAuth to swagger
    /// </summary>
    /// <param name="options">Swagger options</param>
    /// <param name="oauth">OAuth configuration</param>
    public static void AddSwaggerOAuthSecurity(this SwaggerGenOptions options, OAuthConfiguration oauth)
    {
        options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                ClientCredentials = new OpenApiOAuthFlow
                {
                    TokenUrl = new Uri($"{oauth.Authority}/protocol/openid-connect/token"),
                    Scopes = new Dictionary<string, string>
                    {
                        { oauth.Audience, "API access" }
                    }
                }
            }
        });

        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("oauth2", document)] = []
        });
    }
}
