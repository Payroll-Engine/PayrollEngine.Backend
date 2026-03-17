using System;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace PayrollEngine.Api.Core;

/// <summary>The Payroll API server configuration</summary>
public class PayrollServerConfiguration
{
    /// <summary>
    /// Enable Swagger UI and JSON endpoint (default: false).
    /// <para>
    /// Should be disabled in production to prevent exposing the full API surface,
    /// interactive "Try it out" functionality, and OAuth client credentials.
    /// </para>
    /// </summary>
    public bool EnableSwagger { get; set; }

    /// <summary>Use swagger dark theme</summary>
    public bool DarkTheme { get; set; }

    /// <summary>Authentication configuration</summary>
    public AuthenticationConfiguration Authentication { get; set; } = new();

    /// <summary>Culture at start (default: os working culture)</summary>
    public string StartupCulture { get; set; }

    /// <summary>Audit trail configuration</summary>
    public AuditTrailConfiguration AuditTrail { get; set; } = new();

    /// <summary>https redirection (default: false)</summary>
    public bool HttpsRedirection { get; set; }

    /// <summary>Log http request (default: false)</summary>
    public bool LogHttpRequests { get; set; }

    /// <summary>Initialize the script compiler to reduce first execution time (default: false)</summary>
    public bool InitializeScriptCompiler { get; set; }

    /// <summary>Dump compiler source files (default: false)</summary>
    public bool DumpCompilerSources { get; set; }

    /// <summary>
    /// Enable static safety analysis of user scripts during compilation (default: false).
    /// When enabled, scripts are checked for banned API usage (System.IO, System.Net,
    /// System.Diagnostics, System.Reflection, etc.) before the assembly is emitted.
    /// </summary>
    public bool ScriptSafetyAnalysis { get; set; }

    /// <summary>Database command timeout in seconds (default: 2 minutes)</summary>
    public TimeSpan DbCommandTimeout { get; set; } = TimeSpan.FromMinutes(2);

    /// <summary>Database provider (default: SqlServer). Supported: SqlServer | MySql</summary>
    public string DbProvider { get; set; } = "SqlServer";

    /// <summary>Required database collation (default: SQL_Latin1_General_CP1_CS_AS)</summary>
    public string DbCollation { get; set; }

    /// <summary>Database transaction timeout (default: 10 minutes)</summary>
    public TimeSpan DbTransactionTimeout { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>Webhook timeout (default: 1 minutes)</summary>
    public TimeSpan WebhookTimeout { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>Function log timeout</summary>
    public TimeSpan FunctionLogTimeout { get; set; }

    /// <summary>Log employee processing timing (started, per-employee duration, completed summary). Default: false</summary>
    public bool LogEmployeeTiming { get; set; }

    /// <summary>
    /// Maximum degree of parallelism for result persistence (SemaphoreSlim count).
    /// 1 = fully serialized (default, safest, no deadlocks)
    /// 2 to N = parallel persist threads; higher values reduce semWait but increase deadlock risk.
    /// Monitor DB load and test for deadlocks before increasing beyond 2.
    /// </summary>
    public int MaxParallelPersist { get; set; } = 2;

    /// <summary>Assembly cache timeout (default: 30 minutes)</summary>
    public TimeSpan AssemblyCacheTimeout { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Maximum degree of parallelism for employee processing.
    /// null or empty or 0 = auto (default, based on Environment.ProcessorCount)
    /// off or -1 = sequential (no parallelism, useful for debugging)
    /// half = half CPU load, good balance
    /// max = full CPU utilization; monitor DB load
    /// 1 to N = explicit maximum thread count.
    /// </summary>
    public string MaxParallelEmployees { get; init; }

    public int GetMaxParallelEmployees()
    {
        int count;
        switch (MaxParallelEmployees?.ToLower())
        {
            case null:
            case "":
            case "0":
                count = Environment.ProcessorCount;
                break;
            case "off":
            case "-1":
                count = 0;
                break;
            case "half":
                count = Math.Max(1, Environment.ProcessorCount / 2);
                break;
            case "max":
                count = Environment.ProcessorCount;
                break;
            default:
                if (!int.TryParse(MaxParallelEmployees, out count) || count < 1)
                {
                    throw new ArgumentException($"Invalid MaxParallelEmployees value: {MaxParallelEmployees}");
                }
                break;
        }
        return count;
    }

    /// <summary>
    /// Maximum number of retro payrun periods per employee.
    /// Acts as a safety guard to prevent runaway retro calculations when
    /// <c>RetroTimeType.Anytime</c> is configured or a script sets a very old retro date.
    /// 0 = unlimited (default, no guard).
    /// </summary>
    public int MaxRetroPayrunPeriods { get; set; }

    /// <summary>
    /// CORS configuration (default: inactive).
    /// When no allowed origins are specified, only same-origin requests are accepted.
    /// </summary>
    public CorsConfiguration Cors { get; set; } = new();

    /// <summary>
    /// Rate limiting configuration (default: inactive).
    /// Supports a global policy for all endpoints and a dedicated policy
    /// for the payrun job start endpoint.
    /// </summary>
    public RateLimitingConfiguration RateLimiting { get; set; } = new();

    /// <summary>The XML comment files</summary>
    public string[] XmlCommentFileNames { get; set; }

    /// <summary>The visible controllers</summary>
    public string[] VisibleControllers { get; set; }

    /// <summary>The hidden controllers</summary>
    public string[] HiddenControllers { get; set; }
}