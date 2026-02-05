using System;
using System.Net;
using System.Linq;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Serilog;
using Serilog.Events;

namespace PayrollEngine.Api.Core;

public static class LogExtensions
{
    private const LogLevel SystemInfoLogEventLevel = LogLevel.Information;

    public static void UseLog(this IHostApplicationLifetime appLifetime, IConfiguration configuration,
        IApplicationBuilder appBuilder, IHostEnvironment environment, bool logRequests)
    {
        // start
        appLifetime.ApplicationStarted.Register(() =>
        {
            var dbInfo = GetConnectionInfo(configuration);
            Log.Information($"{environment.ApplicationName} > {GetApplicationAddress(appBuilder)} [database: {dbInfo.Server} > {dbInfo.Database}].");
            if (Log.IsEnabled(SystemInfoLogEventLevel))
            {
                Log.Write(SystemInfoLogEventLevel, $"Culture: {CultureInfo.CurrentCulture}");
            }
        });
        // stopping
        appLifetime.ApplicationStopping.Register(() =>
        {
            Log.Information($"{environment.ApplicationName} is stopping...");
        });
        // stopped
        appLifetime.ApplicationStopped.Register(() =>
        {
            Log.Information($"{environment.ApplicationName} stopped.");
        });

        if (logRequests)
        {
            // see https://github.com/andrewlock/blog-examples/blob/master/SerilogRequestLogging/LogHelper.cs
            appBuilder.UseSerilogRequestLogging(opts =>
            {
                opts.EnrichDiagnosticContext = EnrichFromRequest;
                opts.GetLevel = GetLevel(LogEventLevel.Verbose, "Health checks");
            });
        }
    }

    private static (string Server, string Database) GetConnectionInfo(IConfiguration configuration)
    {
        var connectionString = configuration.GetDatabaseConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return (null, null);
        }

        string server = null;
        string database = null;
        var tokens = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);
        foreach (var token in tokens)
        {
            var valueTokens = token.Split('=', StringSplitOptions.RemoveEmptyEntries);
            if (valueTokens.Length != 2)
            {
                continue;
            }

            var name = valueTokens[0].Trim();
            var value = valueTokens[1].Trim();
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            // server
            if (string.Equals(name, "Server", StringComparison.InvariantCultureIgnoreCase))
            {
                server = value;
                continue;
            }

            // database
            if (string.Equals(name, "Database", StringComparison.InvariantCultureIgnoreCase))
            {
                database = value;
            }
        }

        return (server, database);
    }

    private static string GetApplicationAddress(IApplicationBuilder appBuilder)
    {
        var address = appBuilder?.ServerFeatures.Get<IServerAddressesFeature>()?.Addresses.FirstOrDefault();
        if (address == null)
        {
            return string.Empty;
        }
        return address.RemoveFromEnd("/");
    }

    private static void EnrichFromRequest(IDiagnosticContext diagnosticContext, HttpContext httpContext)
    {
        var request = httpContext.Request;
        diagnosticContext.Set("Host", request.Host);
        diagnosticContext.Set("Protocol", request.Protocol);
        diagnosticContext.Set("Scheme", request.Scheme);

        // query string
        if (!string.IsNullOrWhiteSpace(request.QueryString.Value))
        {
            diagnosticContext.Set("QueryString", request.QueryString.Value);
        }

        // content type
        if (!string.IsNullOrWhiteSpace(httpContext.Response.ContentType))
        {
            diagnosticContext.Set("ContentType", httpContext.Response.ContentType);
        }

        // endpoint
        var endpoint = httpContext.GetEndpoint();
        if (!string.IsNullOrWhiteSpace(endpoint?.DisplayName))
        {
            diagnosticContext.Set("EndpointName", endpoint.DisplayName);
        }
    }

    /// <summary>
    /// Create a <see cref="Serilog.AspNetCore.RequestLoggingOptions.GetLevel" /> method that
    /// uses the default logging level, except for the specified endpoint names, which are
    /// logged using the provided <paramref name="traceLevel" />.
    /// </summary>
    /// <param name="traceLevel">The level to use for logging "trace" endpoints</param>
    /// <param name="traceEndpointNames">The display name of endpoints to be considered "trace" endpoints</param>
    private static Func<HttpContext, double, Exception, LogEventLevel> GetLevel(LogEventLevel traceLevel, params string[] traceEndpointNames)
    {
        if (traceEndpointNames is null || traceEndpointNames.Length == 0)
        {
            throw new ArgumentNullException(nameof(traceEndpointNames));
        }

        return (httpContext, _, exception) =>
            IsError(httpContext, exception) ?
                LogEventLevel.Error :
                IsTraceEndpoint(httpContext, traceEndpointNames) ?
                    traceLevel :
                    LogEventLevel.Information;
    }

    private static bool IsError(HttpContext httpContext, Exception exception)
        => exception != null || httpContext.Response.StatusCode >= (int)HttpStatusCode.InternalServerError;

    private static bool IsTraceEndpoint(HttpContext ctx, string[] traceEndpoints)
    {
        var endpoint = ctx.GetEndpoint();
        if (endpoint != null)
        {
            foreach (var traceEndpoint in traceEndpoints)
            {
                if (string.Equals(traceEndpoint, endpoint.DisplayName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }
        return false;
    }
}