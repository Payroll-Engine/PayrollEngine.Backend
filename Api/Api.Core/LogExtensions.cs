using System;
using System.Globalization;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace PayrollEngine.Api.Core;

public static class LogExtensions
{
    private const LogLevel SystemInfoLogEventLevel = LogLevel.Information;

    public static void UseLog(this IHostApplicationLifetime appLifetime,
        IApplicationBuilder appBuilder, IHostEnvironment environment, bool logRequests)
    {
        // start
        appLifetime.ApplicationStarted.Register(() =>
        {
            Log.Information($"{environment.ApplicationName} started");
            if (Log.IsEnabled(SystemInfoLogEventLevel))
            {
                Log.Write(SystemInfoLogEventLevel, $"Current culture: {CultureInfo.CurrentCulture}");
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
            Log.Information($"{environment.ApplicationName} stopped");
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

    private static void EnrichFromRequest(IDiagnosticContext diagnosticContext, HttpContext httpContext)
    {
        var request = httpContext.Request;
        diagnosticContext.Set("Host", request.Host);
        diagnosticContext.Set("Protocol", request.Protocol);
        diagnosticContext.Set("Scheme", request.Scheme);
        if (request.QueryString.HasValue)
        {
            diagnosticContext.Set("QueryString", request.QueryString.Value);
        }
        diagnosticContext.Set("ContentType", httpContext.Response.ContentType);

        var endpoint = httpContext.GetEndpoint();
        if (endpoint != null)
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
    /// <returns></returns>
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