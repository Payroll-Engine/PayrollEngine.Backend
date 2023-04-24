using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using PayrollEngine.Serilog;
using Serilog;

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
        CreateWebHostBuilder(args).Build().Run();
    }

    /// <summary>
    /// Create the web host builder
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static IHostBuilder CreateWebHostBuilder(string[] args)
    {
        var config = new ConfigurationBuilder().AddCommandLine(args).Build();
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>()
                    .UseConfiguration(config);
            })
            .UseSerilog((hostingContext, loggerConfiguration) =>
                loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration));

        // logger
        Log.SetLogger(new PayrollLog());

        return builder;
    }
}