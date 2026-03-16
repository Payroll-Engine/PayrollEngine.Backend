namespace PayrollEngine.Api.Model;

/// <summary>Backend runtime configuration information</summary>
public class BackendRuntimeInformation
{
    /// <summary>Resolved maximum parallel employee threads (0 = sequential)</summary>
    public int MaxParallelEmployees { get; init; }

    /// <summary>Maximum retro payrun periods (0 = unlimited)</summary>
    public int MaxRetroPayrunPeriods { get; init; }

    /// <summary>Database command timeout in seconds</summary>
    public int DbCommandTimeoutSeconds { get; init; }

    /// <summary>Database transaction timeout in seconds</summary>
    public int DbTransactionTimeoutSeconds { get; init; }

    /// <summary>Script assembly cache timeout in seconds</summary>
    public int AssemblyCacheTimeoutSeconds { get; init; }

    /// <summary>Webhook call timeout in seconds</summary>
    public int WebhookTimeoutSeconds { get; init; }

    /// <summary>Static safety analysis of user scripts enabled</summary>
    public bool ScriptSafetyAnalysis { get; init; }

    /// <summary>Audit trail feature flags</summary>
    public BackendAuditTrailInformation AuditTrail { get; init; }

    /// <summary>CORS configuration</summary>
    public BackendCorsInformation Cors { get; init; }

    /// <summary>Rate limiting configuration</summary>
    public BackendRateLimitingInformation RateLimiting { get; init; }
}
