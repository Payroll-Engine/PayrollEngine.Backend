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

    /// <summary>
    /// Api request middleware
    /// </summary>
    /// <param name="next">Next middleware</param>
    /// <param name="configuration"></param>
    public ApiRequestMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        Next = next;
        ApiKey = configuration.GetApiKey();
    }

    /// <summary>
    /// Invoke middleware including the api key test
    /// </summary>
    /// <remarks>Swagger pages are excluded from the api key test</remarks>
    /// <param name="context">Http context</param>
    /// <param name="configuration">Application configuration (injected by DI)</param>
    public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
    {
        // root redirect
        if ("/".Equals(context.Request.Path.Value))
        {
            context.Response.Redirect("/swagger");
            return;
        }

        // test api key, excluding swagger requests
        if (!string.IsNullOrWhiteSpace(ApiKey) &&
            !context.Request.Path.StartsWithSegments("/swagger"))
        {
            if (!context.Request.Headers.TryGetValue(BackendSpecification.ApiKeyHeader, out var headerApiKey)
                || !string.Equals(ApiKey, headerApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid Api Key");
                return;
            }
        }

        // next middleware
        await Next(context);
    }
}