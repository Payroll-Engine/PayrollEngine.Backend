using Microsoft.Extensions.Configuration;
using PayrollEngine.Domain.Scripting;
using Microsoft.Extensions.DependencyInjection;
using PayrollEngine.Persistence.SqlServer;
using PayrollEngine.Domain.Model;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Api.Core;

internal static class ApiFactory
{
    // services setup
    internal static void SetupApiServices(IServiceCollection services, IConfiguration configuration)
    {
        // database connection string
        var connectionString = Task.Run(configuration.GetConnectionStringAsync).Result;
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new PayrollException("Missing database connection string");
        }
        // database command timeout
        var serverConfiguration = configuration.GetConfiguration<PayrollServerConfiguration>();

        // test database
        var dbContext = new DbContext(connectionString, serverConfiguration.DbCommandTimeout);
        if (!Task.Run(dbContext.TestVersionAsync).Result)
        {
            throw new PayrollException("Invalid database version");
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