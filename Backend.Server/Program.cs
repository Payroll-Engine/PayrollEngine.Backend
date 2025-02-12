using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
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
    /// <param name="args"></param>
    public static void Main(string[] args)
    {
        // configuration
        var configuration = new ConfigurationBuilder()
            .AddCommandLine(args)
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
            .AddUserSecrets(typeof(Program).Assembly)
            .Build();

        // system logger
        SysLog.Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        try
        {
            CreateWebHostBuilder(configuration, args).Build().Run();
        }
        catch (Exception exception)
        {
            Log.Critical(exception, "Application terminated unexpectedly");
        }
        finally
        {
            SysLog.Log.CloseAndFlush();
        }
    }

    /// <summary>
    /// Create the web host builder
    /// </summary>
    private static IHostBuilder CreateWebHostBuilder(IConfiguration configuration, string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>()
                    .UseConfiguration(configuration);
            })
            .UseSerilog((hostingContext, loggerConfiguration) =>
                loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration));

        // logger
        Log.SetLogger(new PayrollLog());
        return builder;
    }
}