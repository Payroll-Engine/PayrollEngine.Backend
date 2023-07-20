using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PayrollEngine.Api.Core;

public static class HealthCheckExtensions
{
    private static readonly string ReadyTag = "ready";

    private static readonly Uri ReadyUrl = new("/health/ready");
    private static readonly Uri LiveUrl = new("/health/live");
    private static readonly Uri UserInterfaceUrl = new("/health/ui");

    /// <summary>
    /// Setup API health check
    /// </summary>
    public static async Task AddApiHealthCheckAsync(this IServiceCollection services, IConfiguration configuration)
    {
        // database connection string
        var connectionString = await configuration.GetSharedConnectionStringAsync();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Log.Error("API health check setup: missing database connection string");
            return;
        }

        services.AddHealthChecks()
            .AddSqlServer(connectionString, failureStatus: HealthStatus.Unhealthy, tags: new[] { ReadyTag });
        services.AddHealthChecksUI()
            .AddInMemoryStorage();
    }

    /// <summary>
    /// Uses the API health check.
    /// See https://github.com/ricardodemauro/Health-Check-Series/blob/master/Startup.cs
    /// </summary>
    public static void UseHealthCheck(this IApplicationBuilder appBuilder)
    {
        appBuilder.UseEndpoints(endpoints =>
        {
            // health ready
            endpoints.MapHealthChecks(ReadyUrl.AbsoluteUri, new()
            {
                ResultStatusCodes =
                {
                    [HealthStatus.Healthy] = StatusCodes.Status200OK,
                    [HealthStatus.Degraded] = StatusCodes.Status500InternalServerError,
                    [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                },
                ResponseWriter = WriteHealthCheckReadyResponse,
                Predicate = check => check.Tags.Contains(ReadyTag),
                AllowCachingResponses = false
            });

            // health live
            endpoints.MapHealthChecks(LiveUrl.AbsoluteUri, new()
            {
                Predicate = check => !check.Tags.Contains(ReadyTag),
                ResponseWriter = WriteHealthCheckLiveResponse,
                AllowCachingResponses = false
            });

            // health UI
            endpoints.MapHealthChecks(UserInterfaceUrl.AbsoluteUri, new()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            endpoints.MapHealthChecksUI();
        });

    }

    private static Task WriteHealthCheckLiveResponse(HttpContext httpContext, HealthReport result) =>
        httpContext.Response.WriteAsync(GetResponseJson(httpContext, result, false));

    private static Task WriteHealthCheckReadyResponse(HttpContext httpContext, HealthReport result) =>
        httpContext.Response.WriteAsync(GetResponseJson(httpContext, result, true));

    private static string GetResponseJson(HttpContext httpContext, HealthReport result, bool includeDependencies)
    {
        // ensure UTF8
        httpContext.Response.ContentType = ContentType.JsonUtf8;

        // response stream
        using var stream = new MemoryStream();
        // json writer
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
        {
            // health base data
            writer.WriteStartObject();
            writer.WriteString("OverallStatus", result.Status.ToString());
            writer.WriteString("TotalChecksDuration", result.TotalDuration.TotalSeconds.ToString("0:0.00", CultureInfo.InvariantCulture));

            // health depend data
            if (includeDependencies)
            {
                writer.WriteStartObject("DependencyHealthChecks");
                foreach (var entry in result.Entries)
                {
                    writer.WriteStartObject(entry.Key);
                    writer.WriteString("Status", entry.Value.Status.ToString());
                    writer.WriteString("Description", entry.Value.Description);
                    writer.WriteString("Duration", entry.Value.Duration.TotalSeconds.ToString("0:0.00", CultureInfo.InvariantCulture));
                    writer.WriteString("Exception", entry.Value.Exception?.GetBaseMessage());
                    writer.WriteStartObject("Data");
                    foreach (var item in entry.Value.Data)
                    {
                        writer.WritePropertyName(item.Key);
                        JsonSerializer.Serialize(writer, item.Value, item.Value.GetType());
                    }
                    writer.WriteEndObject();
                    writer.WriteEndObject();
                }
                writer.WriteEndObject();
            }
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }
}