// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace PayrollEngine.Api.Core;

/// <summary>The Payroll API server configuration</summary>
public class AuditTrailConfiguration
{
    /// <summary>Use audit trail for scripts</summary>
    public bool Script { get; set; }

    /// <summary>Use audit trail for lookup and lookup values</summary>
    /// <remarks>Audit trail is not supported on bulk values import</remarks>
    public bool Lookup { get; set; }

    /// <summary>Use audit trail for cases, case fields and case relations</summary>
    public bool Input { get; set; }

    /// <summary>Use audit trail for collectors and wage types</summary>
    public bool Payrun { get; set; }

    /// <summary>Use audit trail for reports, report templates and report parameters</summary>
    public bool Report { get; set; }
}