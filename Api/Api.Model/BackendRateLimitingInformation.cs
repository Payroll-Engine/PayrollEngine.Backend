namespace PayrollEngine.Api.Model;

/// <summary>Rate limiting runtime information</summary>
public class BackendRateLimitingInformation
{
    /// <summary>Whether any rate limiting policy is active</summary>
    public bool IsActive { get; init; }

    /// <summary>Global policy: max requests per window (0 = inactive)</summary>
    public int GlobalPermitLimit { get; init; }

    /// <summary>Global policy: window size in seconds</summary>
    public int GlobalWindowSeconds { get; init; }

    /// <summary>Payrun job start policy: max requests per window (0 = inactive)</summary>
    public int PayrunJobStartPermitLimit { get; init; }

    /// <summary>Payrun job start policy: window size in seconds</summary>
    public int PayrunJobStartWindowSeconds { get; init; }
}
