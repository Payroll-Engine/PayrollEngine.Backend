using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using SysLog = Serilog;
using PayrollEngine.Serilog;

namespace PayrollEngine.Backend.Server;

/// <summary>
/// Start program for the provider api
/// </summary>
public static class Program
{
    /// <summary>
    /// The program entry point
    /// </summary>
    public static void Main(string[] args)
    {
        // Bootstrap logger: active before Host.Build() so that startup exceptions
        // (e.g. DB version mismatch, missing connection string) are written to the
        // log file. Replaced by the full Serilog configuration from appsettings.json
        // once UseSerilog() runs during Build().
        //
        // Configuration is built manually here so that User Secrets and environment
        // variables are available to the bootstrap logger — identical to what
        // CreateDefaultBuilder() would load during the actual host build.
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        var bootstrapConfig = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .AddUserSecrets(typeof(Program).Assembly, optional: true)
            .Build();

        SysLog.Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(bootstrapConfig)
            .CreateBootstrapLogger();

        try
        {
            Log.SetLogger(new PayrollLog());

            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .UseSerilog((context, _, loggerConfiguration) =>
                    loggerConfiguration.ReadFrom.Configuration(context.Configuration))
                .Build()
                .Run();
        }
        catch (Exception exception)
        {
            SysLog.Log.Fatal(exception, "STARTUP EXCEPTION: {Message}", exception.GetBaseException().Message);
            Console.Error.WriteLine($"STARTUP EXCEPTION: {exception}");
        }
        finally
        {
            SysLog.Log.CloseAndFlush();
        }
    }
}