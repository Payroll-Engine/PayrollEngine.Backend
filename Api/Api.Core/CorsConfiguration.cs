namespace PayrollEngine.Api.Core;

/// <summary>
/// Cross-Origin Resource Sharing (CORS) configuration.
/// When no <see cref="AllowedOrigins"/> are specified, CORS is inactive
/// and the API only accepts same-origin requests.
/// </summary>
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
public class CorsConfiguration
{
    /// <summary>CORS policy name used by the middleware</summary>
    internal const string PolicyName = "PayrollCors";

    /// <summary>
    /// Allowed origins (e.g. "https://app.example.com").
    /// Leave empty to disable CORS (default).
    /// Use <c>["*"]</c> only for development / fully public APIs.
    /// </summary>
    public string[] AllowedOrigins { get; set; } = [];

    /// <summary>
    /// Allowed HTTP methods (default: GET, POST, PUT, DELETE, PATCH, OPTIONS).
    /// </summary>
    public string[] AllowedMethods { get; set; } =
        ["GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS"];

    /// <summary>
    /// Allowed request headers (default: Content-Type, Authorization, Api-Key, Auth-Tenant).
    /// </summary>
    public string[] AllowedHeaders { get; set; } =
        ["Content-Type", "Authorization", "Api-Key", BackendSpecification.TenantAuthorizationHeader];

    /// <summary>
    /// Whether the browser should include credentials (cookies, authorization headers)
    /// in cross-origin requests (default: false).
    /// Cannot be combined with a wildcard origin.
    /// </summary>
    public bool AllowCredentials { get; set; }

    /// <summary>
    /// How long (in seconds) the browser may cache the preflight response (default: 600 = 10 minutes).
    /// </summary>
    public int PreflightMaxAgeSeconds { get; set; } = 600;

    /// <summary>Whether any allowed origin is configured</summary>
    public bool IsActive => AllowedOrigins is { Length: > 0 };
}
