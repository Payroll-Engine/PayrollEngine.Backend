using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using PayrollEngine.Api.Model;
using PayrollEngine.Domain.Scripting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace PayrollEngine.Api.Core;

public abstract class AdminController(IControllerRuntime runtime, IHostApplicationLifetime appLifetime)
    : ApiController(runtime)
{
    private IHostApplicationLifetime ApplicationLifetime { get; } = appLifetime ?? throw new ArgumentNullException(nameof(appLifetime));

    /// <summary>
    /// Requests termination of the API application
    /// </summary>
    /// <remarks>
    /// In IIS the application will be restarted with the next API request
    /// source https://edi.wang/post/2019/3/7/restart-an-aspnet-core-application-programmatically
    /// </remarks>
    /// <returns>Ok</returns>
    public virtual ActionResult StopApplication()
    {
        Log.Information("Stopping the application");
        ApplicationLifetime.StopApplication();
        return Ok();
    }

    /// <summary>
    /// Requests termination of the API application
    /// </summary>
    /// <remarks>
    /// In IIS the application will be restarted with the next API request
    /// source https://edi.wang/post/2019/3/7/restart-an-aspnet-core-application-programmatically
    /// </remarks>
    /// <returns>Ok</returns>
    public virtual ActionResult ClearApplicationCache()
    {
        AssemblyCache.CacheClear();
        return Ok();
    }

    /// <summary>
    /// Get query method names
    /// </summary>
    /// <returns>List of web method names</returns>
    public virtual ActionResult<IEnumerable<string>> GetApiReportMethods()
    {
        return Ok(ApiQueryFactory.GetQueryNames());
    }

    /// <summary>
    /// Get backend server information
    /// </summary>
    /// <returns>Backend information</returns>
    public virtual async Task<ActionResult<BackendInformation>> GetBackendInformationAsync()
    {
        var assembly = GetType().Assembly;
        var version = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion ?? "unknown";
        var buildDate = System.IO.File.Exists(assembly.Location)
            ? System.IO.File.GetLastWriteTimeUtc(assembly.Location)
            : DateTime.UtcNow;

        var serverConfig = Configuration.GetConfiguration<PayrollServerConfiguration>() ?? new();
        var dbInfo = await Runtime.DbContext.GetDatabaseInformationAsync();
        var auth = serverConfig.Authentication;

        return Ok(new BackendInformation
        {
            Version = version,
            BuildDate = buildDate,
            ApiVersion = BackendSpecification.CurrentApiVersion.ToString(),
            ApiName = BackendSpecification.ApiName,
            Authentication = new BackendAuthInformation
            {
                Mode = auth.Mode.ToString(),
                OAuthAuthority = auth.Mode == AuthenticationMode.OAuth ? auth.OAuth?.Authority : null,
                OAuthAudience = auth.Mode == AuthenticationMode.OAuth ? auth.OAuth?.Audience : null
            },
            Database = new BackendDatabaseInformation
            {
                Type = dbInfo.Type,
                Name = dbInfo.Name,
                Version = dbInfo.Version,
                Edition = dbInfo.Edition
            },
            Runtime = new BackendRuntimeInformation
            {
                MaxParallelEmployees = serverConfig.GetMaxParallelEmployees(),
                MaxRetroPayrunPeriods = serverConfig.MaxRetroPayrunPeriods,
                DbCommandTimeoutSeconds = (int)serverConfig.DbCommandTimeout.TotalSeconds,
                DbTransactionTimeoutSeconds = (int)serverConfig.DbTransactionTimeout.TotalSeconds,
                AssemblyCacheTimeoutSeconds = (int)serverConfig.AssemblyCacheTimeout.TotalSeconds,
                WebhookTimeoutSeconds = (int)serverConfig.WebhookTimeout.TotalSeconds,
                ScriptSafetyAnalysis = serverConfig.ScriptSafetyAnalysis,
                AuditTrail = new BackendAuditTrailInformation
                {
                    Script = serverConfig.AuditTrail.Script,
                    Lookup = serverConfig.AuditTrail.Lookup,
                    Input = serverConfig.AuditTrail.Input,
                    Payrun = serverConfig.AuditTrail.Payrun,
                    Report = serverConfig.AuditTrail.Report
                },
                Cors = new BackendCorsInformation
                {
                    IsActive = serverConfig.Cors.IsActive,
                    AllowedOrigins = serverConfig.Cors.AllowedOrigins
                },
                RateLimiting = new BackendRateLimitingInformation
                {
                    IsActive = serverConfig.RateLimiting.IsActive,
                    GlobalPermitLimit = serverConfig.RateLimiting.Global.PermitLimit,
                    GlobalWindowSeconds = serverConfig.RateLimiting.Global.WindowSeconds,
                    PayrunJobStartPermitLimit = serverConfig.RateLimiting.PayrunJobStart.PermitLimit,
                    PayrunJobStartWindowSeconds = serverConfig.RateLimiting.PayrunJobStart.WindowSeconds
                }
            }
        });
    }
}