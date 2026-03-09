namespace PayrollEngine.Api.Core;

/// <summary>
/// Configuration for a single fixed-window rate limiting policy.
/// When <see cref="PermitLimit"/> is 0 (default), the policy is inactive.
/// </summary>
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
public class RateLimitPolicy
{
    /// <summary>
    /// Maximum number of requests permitted within the time window.
    /// 0 = inactive (default), no rate limiting applied.
    /// </summary>
    public int PermitLimit { get; set; }

    /// <summary>
    /// Time window in seconds (default: 60).
    /// </summary>
    public int WindowSeconds { get; set; } = 60;

    /// <summary>Whether this policy is active</summary>
    public bool IsActive => PermitLimit > 0;
}