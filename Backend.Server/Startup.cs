using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PayrollEngine.Api.Core;

namespace PayrollEngine.Backend.Server;

/// <summary>
/// Api startup setup
/// </summary>
public class Startup
{
    private static readonly ApiSpecification apiSpecification = new(
        apiDocumentationName: "PayrollEngineBackendAPI",
        apiDocumentationFileName: "BackendApi.xml",
        apiName: BackendSpecification.ApiName,
        apiDescription: BackendSpecification.ApiDescription,
        apiVersion: BackendSpecification.CurrentApiVersion.ToString(1));

    /// <summary>
    /// Api startup constructor
    /// </summary>
    /// <param name="configuration"></param>
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    /// <summary>
    /// The application configuration
    /// </summary>
    private IConfiguration Configuration { get; }

    /// <summary>
    /// Service configuration
    /// This method gets called by the runtime. Use this method to add services to the container. 
    /// </summary>
    public void ConfigureServices(IServiceCollection services)
    {
        // database connection string
        var connectionString = Configuration.GetDatabaseConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new PayrollException("Missing database connection string.");
        }

        // configuration
        var serverConfiguration = Configuration.GetConfiguration<PayrollServerConfiguration>();

        // database command timeout
        var dbContext = new Persistence.SqlServer.DbContext(
            connectionString: connectionString,
            defaultCommendTimeout: Convert.ToInt32(serverConfiguration.DbCommandTimeout.TotalSeconds));

        // setup services
        services.AddApiServices(
            configuration: Configuration,
            controllerVisibility: new ControllerVisibility(),
            specification: apiSpecification,
            dbContext: dbContext);
        services.AddLocalApiServices();
    }

    /// <summary>
    /// Application configuration
    /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    /// </summary>
    public void Configure(IApplicationBuilder appBuilder, IWebHostEnvironment environment,
        IHostApplicationLifetime appLifetime) =>
        appBuilder.UsePayrollApiServices(
            environment: environment,
            appLifetime: appLifetime,
            configuration: Configuration,
            specification: apiSpecification);
}