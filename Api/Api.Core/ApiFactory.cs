using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PayrollEngine.Domain.Application;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Scripting;

namespace PayrollEngine.Api.Core;

/// <summary>
/// Core API factory that registers the database context, controller runtime,
/// background services, and delegates to repository/service sub-factories.
/// </summary>
internal static class ApiFactory
{
    /// <summary>
    /// Register all core API services: database context, controller runtime,
    /// payrun job queue, webhook HttpClient, query service, and all repository/service factories.
    /// </summary>
    /// <param name="services">Service collection to configure</param>
    /// <param name="configuration">Application configuration for connection strings and server settings</param>
    /// <param name="dbContext">Database context instance for persistence operations</param>
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

        // test database — skipped during Swashbuckle CLI swagger generation (no real DB available)
        var swaggerGeneration = !string.IsNullOrEmpty(
            System.Environment.GetEnvironmentVariable("PAYROLL_SWAGGER_GENERATION"));
        if (!swaggerGeneration)
        {
            var exception = dbContext.TestVersionAsync().GetAwaiter().GetResult();
            if (exception != null)
            {
                Log.Critical(exception, exception.GetBaseException().Message);
                return;
            }
        }
        services.AddTransient(_ => dbContext);

        // api controller runtime context
        services.AddScoped<IControllerRuntime, ControllerRuntime>();

        // Payrun job background processing
        services.AddSingleton<IPayrunJobQueue, PayrunJobQueue>();
        services.AddHostedService<PayrunJobWorkerService>();

        // Named HttpClient for webhook dispatch (managed pooling, DNS rotation)
        // fall back to defaults when no section is present (e.g. Swashbuckle CLI)
        var serverConfiguration = configuration.GetConfiguration<PayrollServerConfiguration>() ?? new();
        services.AddHttpClient(WebhookDispatchService.HttpClientName, client =>
        {
            client.Timeout = serverConfiguration.WebhookTimeout;
        });

        // api query service: singleton to reduce assembly reflection on each query
        services.AddSingleton<IQueryService, QueryService>();
        // hot spot for custom payroll calculators
        services.AddSingleton<IPayrollCalculatorProvider, DefaultPayrollCalculatorProvider>();

        // repositories
        ApiRepositoryFactory.SetupApiServices(services, configuration);
        ApiServiceFactory.SetupApiServices(services);
    }
}