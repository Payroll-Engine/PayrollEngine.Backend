using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PayrollEngine.Api.Core;
using PayrollEngine.Persistence;
using PayrollEngine.Persistence.SqlServer;

namespace PayrollEngine.Backend.Api;

/// <summary>
/// Api startup setup
/// </summary>
public class Startup
{
    private static readonly ApiSpecification apiSpecification = new(
        apiDocumentationName: "PayrollEngineBackendAPI",
        apiDescription: "Payroll Engine API",
        apiName: "Payroll Engine Backend API",
        apiDocumentationFileName: "BackendApi.xml",
        apiVersion: "1");

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
    public IConfiguration Configuration { get; }

    /// <summary>
    /// Service configuration
    /// This method gets called by the runtime. Use this method to add services to the container. 
    /// </summary>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddApiServices(Configuration, apiSpecification, GetDatabaseContext());
        services.AddLocalApiServices();
    }

    /// <summary>Creates the database context</summary>
    /// <returns>The database context</returns>
    private IDbContext GetDatabaseContext()
    {
        var connectionString = Configuration.GetConnectionString(SystemSpecification.DatabaseConnectionString);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new PayrollException($"Missing database connection string {SystemSpecification.DatabaseConnectionString}");
        }

        // SQL Server context
        var context = new DbContext(connectionString);
        return context;
    }

    /// <summary>
    /// Application configuration
    /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    /// </summary>
    public void Configure(IApplicationBuilder appBuilder, IWebHostEnvironment environment,
        IHostApplicationLifetime appLifetime) =>
        appBuilder.UsePayrollApiServices(environment, appLifetime, Configuration, apiSpecification);
}