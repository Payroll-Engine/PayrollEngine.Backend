using System;
using Microsoft.Extensions.Configuration;
using PayrollEngine.Domain.Scripting;
using Microsoft.Extensions.DependencyInjection;
using PayrollEngine.Domain.Model;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Api.Core;

internal static class ApiFactory
{
    // services setup
    internal static void SetupApiServices(IServiceCollection services, 
        IConfiguration configuration, IDbContext dbContext)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }
        if (dbContext == null)
        {
            throw new ArgumentNullException(nameof(dbContext));
        }

        // database connection string
        var connectionString = Task.Run(configuration.GetSharedConnectionStringAsync).Result;
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new PayrollException("Missing database connection string");
        }

        // test database
        if (!Task.Run(dbContext.TestVersionAsync).Result)
        {
            throw new PayrollException("Invalid database version");
        }
        services.AddTransient(_ => dbContext);

        // api controller runtime context
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