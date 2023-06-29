﻿using Microsoft.Extensions.Configuration;
using PayrollEngine.Domain.Scripting;
using Microsoft.Extensions.DependencyInjection;
using PayrollEngine.Domain.Model;
using PayrollEngine.Persistence.SqlServer;

namespace PayrollEngine.Api.Core;

internal static class ApiFactory
{
    // services setup
    internal static void SetupApiServices(IServiceCollection services, IConfiguration configuration)
    {
        // server config
        var serverConfiguration = configuration.GetConfiguration<PayrollServerConfiguration>();

        // database context
        var connectionString = configuration.GetConnectionString(SystemSpecification.DatabaseConnectionString);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new PayrollException($"Missing database connection string {SystemSpecification.DatabaseConnectionString}");
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