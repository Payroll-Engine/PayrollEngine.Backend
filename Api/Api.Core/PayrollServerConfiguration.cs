using System;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace PayrollEngine.Api.Core;

/// <summary>The Payroll API server configuration</summary>
public class PayrollServerConfiguration
{
    /// <summary>Use swagger dark theme</summary>
    public bool DarkTheme { get; set; }

    /// <summary>Api key (default: none)</summary>
    public string ApiKey { get; set; }

    /// <summary>Culture at start (default: os working culture)</summary>
    public string StartupCulture { get; set; }

    /// <summary>Disable the audit trail (default: false)</summary>
    public bool AuditTrailDisabled { get; set; }

    /// <summary>https redirection (default: false)</summary>
    public bool HttpsRedirection { get; set; }

    /// <summary>Log http request (default: false)</summary>
    public bool LogHttpRequests { get; set; }

    /// <summary>Initialize the script compiler to reduce first execution time (default: false)</summary>
    public bool InitializeScriptCompiler { get; set; }

    /// <summary>Dump compiler source files (default: false)</summary>
    public bool DumpCompilerSources { get; set; }

    /// <summary>Database command timeout in seconds (default: 2 minutes)</summary>
    public TimeSpan DbCommandTimeout { get; set; } = TimeSpan.FromMinutes(2);

    /// <summary>Database transaction timeout</summary>
    public TimeSpan DbTransactionTimeout { get; set; }

    /// <summary>Webhook timeout (default: 1 minutes)</summary>
    public TimeSpan WebhookTimeout { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>Function log timeout</summary>
    public TimeSpan FunctionLogTimeout { get; set; }

    /// <summary>Assembly cache timeout (default: 30 minutes)</summary>
    public TimeSpan AssemblyCacheTimeout { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>The XML comment files</summary>
    public string[] XmlCommentFileNames { get; set; }

    /// <summary>The visible controllers</summary>
    public string[] VisibleControllers { get; set; }

    /// <summary>The hidden controllers</summary>
    public string[] HiddenControllers { get; set; }
}