using System;
using Microsoft.AspNetCore.Hosting;
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
        try
        {
            Log.SetLogger(new PayrollLog());

            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .UseSerilog((context, loggerConfiguration) =>
                    loggerConfiguration.ReadFrom.Configuration(context.Configuration))
                .Build()
                .Run();
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"STARTUP EXCEPTION: {exception}");
            Log.Critical(exception, "Application terminated unexpectedly");
        }
        finally
        {
            SysLog.Log.CloseAndFlush();
        }
    }
}