using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PayrollEngine.Domain.Application;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Scripting;

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
        var connectionString = configuration.GetDatabaseConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Log.Critical("Startup error: missing database connection string.");
            return;
        }

        // test database
        var exception = dbContext.TestVersionAsync().Result;
        if (exception != null)
        {
            Log.Critical(exception, exception.GetBaseException().Message);
            return;
        }
        services.AddTransient(_ => dbContext);

        // api controller runtime context
        services.AddScoped<IControllerRuntime, ControllerRuntime>();

        // Payrun job background processing
        services.AddSingleton<IPayrunJobQueue, PayrunJobQueue>();
        services.AddHostedService<PayrunJobWorkerService>();

        // api query service: singleton to reduce assembly reflection on each query
        services.AddSingleton<IQueryService, QueryService>();
        // hot spot for custom payroll calculators
        services.AddSingleton<IPayrollCalculatorProvider, DefaultPayrollCalculatorProvider>();

        // repositories
        ApiRepositoryFactory.SetupApiServices(services, configuration);
        ApiServiceFactory.SetupApiServices(services, configuration);
    }
}