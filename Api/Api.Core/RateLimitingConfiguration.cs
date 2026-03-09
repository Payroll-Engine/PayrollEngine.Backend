namespace PayrollEngine.Api.Core;

/// <summary>
/// Rate limiting configuration with a global policy and endpoint-specific policies.
/// All policies are inactive by default (PermitLimit = 0).
/// </summary>
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
public class RateLimitingConfiguration
{
    /// <summary>
    /// Global rate limit applied to all endpoints.
    /// Inactive by default.
    /// </summary>
    public RateLimitPolicy Global { get; set; } = new();

    /// <summary>
    /// Rate limit for the payrun job start endpoint
    /// (POST /api/tenants/{tenantId}/payruns/jobs).
    /// Inactive by default.
    /// </summary>
    public RateLimitPolicy PayrunJobStart { get; set; } = new();

    /// <summary>Whether any rate limiting policy is active</summary>
    public bool IsActive => Global.IsActive || PayrunJobStart.IsActive;
}
