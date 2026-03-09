using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
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
    private bool SwaggerEnabled { get; }

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
        SwaggerEnabled = configuration.GetConfiguration<PayrollServerConfiguration>().EnableSwagger;
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

        // root redirect — only when swagger is enabled, skip in OAuth mode (handled by rewriter)
        if ("/".Equals(path.Value) && SwaggerEnabled && AuthMode != AuthenticationMode.OAuth)
        {
            context.Response.Redirect("/swagger");
            return;
        }

        // API key check — skip swagger paths only when swagger is enabled
        if (AuthMode == AuthenticationMode.ApiKey &&
            !(SwaggerEnabled && path.StartsWithSegments("/swagger")))
        {
            if (string.IsNullOrWhiteSpace(ApiKey) ||
                !context.Request.Headers.TryGetValue(BackendSpecification.ApiKeyHeader, out var headerApiKey) ||
                !FixedTimeApiKeyEquals(ApiKey, headerApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid Api Key");
                return;
            }
        }

        // API version response header
        context.Response.Headers[BackendSpecification.ApiVersionHeader] =
            BackendSpecification.CurrentApiVersion.ToString(2);

        await Next(context);
    }

    /// <summary>
    /// Compares the configured API key with the request header value using
    /// a constant-time comparison to prevent timing side-channel attacks.
    /// </summary>
    /// <param name="expected">The configured API key.</param>
    /// <param name="actual">The API key from the request header.</param>
    /// <returns><c>true</c> if the keys match; otherwise <c>false</c>.</returns>
    private static bool FixedTimeApiKeyEquals(string expected, string actual)
    {
        if (string.IsNullOrEmpty(actual))
        {
            return false;
        }

        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        var actualBytes = Encoding.UTF8.GetBytes(actual);
        return CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes);
    }
}