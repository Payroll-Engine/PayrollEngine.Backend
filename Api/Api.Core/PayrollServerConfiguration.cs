using System;

namespace PayrollEngine.Api.Core;

/// <summary>The Payroll API server configuration</summary>
public class PayrollServerConfiguration
{
    /// <summary>Use swagger dark theme</summary>
    public bool DarkTheme { get; set; }

    /// <summary>Culture at start (default: os working culture)</summary>
    public string StartupCulture { get; set; }

    /// <summary>Log http request (default: false)</summary>
    public bool LogHttpRequests { get; set; }

    /// <summary>Initialize the script compiler to reduce first execution time (default: false)</summary>
    public bool InitializeScriptCompiler { get; set; }

    /// <summary>Use health check (default: false)</summary>
    public bool UseHealthCheck { get; set; }

    /// <summary>Webhook timeout</summary>
    public TimeSpan WebhookTimeout { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>Function log timeout</summary>
    public TimeSpan FunctionLogTimeout { get; set; }

    /// <summary>Transaction timeout</summary>
    public TimeSpan TransactionTimeout { get; set; }

    /// <summary>Assembly cache timeout</summary>
    public TimeSpan AssemblyCacheTimeout { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>The XML comment files</summary>
    public string[] XmlCommentFileNames { get; set; }
}