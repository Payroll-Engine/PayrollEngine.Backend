using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace PayrollEngine.Api.Core;

/// <summary>
/// Api key middleware
/// </summary>
public class ApiRequestMiddleware
{
    private RequestDelegate Next { get; }
    private string ApiKey { get; }
    private AuthenticationMode AuthMode { get; }

    /// <summary>
    /// Api request middleware
    /// </summary>
    /// <param name="next">Next middleware</param>
    /// <param name="configuration"></param>
    public ApiRequestMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        Next = next;
        var auth = configuration.GetAuthConfiguration();
        AuthMode = auth.Mode;
        ApiKey = AuthMode == AuthenticationMode.ApiKey ? configuration.GetApiKey() : null;
    }

    /// <summary>
    /// Invoke middleware including the api key test
    /// </summary>
    /// <remarks>Swagger pages are excluded from the api key test</remarks>
    /// <param name="context">Http context</param>
    /// <param name="configuration">Application configuration (injected by DI)</param>
    public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
    {
        var path = context.Request.Path;

        // root redirect — skip in OAuth mode, handled by rewriter
        if ("/".Equals(path.Value) && AuthMode != AuthenticationMode.OAuth)
        {
            context.Response.Redirect("/swagger");
            return;
        }

        // API key check — skip swagger paths
        if (AuthMode == AuthenticationMode.ApiKey &&
            !path.StartsWithSegments("/swagger"))
        {
            if (string.IsNullOrWhiteSpace(ApiKey) ||
                !context.Request.Headers.TryGetValue(BackendSpecification.ApiKeyHeader, out var headerApiKey) ||
                !string.Equals(ApiKey, headerApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid Api Key");
                return;
            }
        }

        await Next(context);
    }
}