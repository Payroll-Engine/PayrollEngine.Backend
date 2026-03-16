namespace PayrollEngine.Api.Model;

/// <summary>Audit trail feature flags</summary>
public class BackendAuditTrailInformation
{
    /// <summary>Script audit trail enabled</summary>
    public bool Script { get; init; }

    /// <summary>Lookup and lookup value audit trail enabled</summary>
    public bool Lookup { get; init; }

    /// <summary>Case, case field and case relation audit trail enabled</summary>
    public bool Input { get; init; }

    /// <summary>Collector and wage type audit trail enabled</summary>
    public bool Payrun { get; init; }

    /// <summary>Report, report template and report parameter audit trail enabled</summary>
    public bool Report { get; init; }
}
