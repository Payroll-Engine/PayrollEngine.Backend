using Microsoft.Extensions.Configuration;
using PayrollEngine.Domain.Scripting;
using Microsoft.Extensions.DependencyInjection;
using PayrollEngine.Domain.Model;
using PayrollEngine.Persistence.SqlServer;
using System;

namespace PayrollEngine.Api.Core;

internal static class ApiFactory
{
    // services setup
    internal static void SetupApiServices(IServiceCollection services, IConfiguration configuration)
    {
        // server config
        var serverConfiguration = configuration.GetConfiguration<PayrollServerConfiguration>();

        // database context
        // priority 1: application configuration
        var connectionString = configuration.GetConnectionString(SystemSpecification.DatabaseConnectionVariable);
        // priority 2: environment variable
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = Environment.GetEnvironmentVariable(SystemSpecification.DatabaseConnectionVariable);
        }
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new PayrollException($"Missing database connection string {SystemSpecification.DatabaseConnectionVariable}");
        }
        services.AddTransient<IDbContext>((_) => new DbContext(connectionString, serverConfiguration.DbCommandTimeout));

        // tenant management and api controller runtime context
        services.AddScoped<ITenantManager, TenantManager>();
        services.AddScoped<IControllerRuntime, ControllerRuntime>();
        // api query service: singleton to reduce assembly reflection on each query
        services.AddSingleton<IQueryService, QueryService>();
        // hot spot for custom payroll calculators
        services.AddSingleton<IPayrollCalculatorProvider, DefaultPayrollCalculatorProvider>();

        // repositories
        ApiRepositoryFactory.SetupApiServices(services);
        ApiServiceFactory.SetupApiServices(services, configuration);
    }
}