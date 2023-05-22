using Microsoft.Extensions.Configuration;
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
        // database context
        var connectionString = configuration.GetConnectionString(SystemSpecification.DatabaseConnectionString);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new PayrollException($"Missing database connection string {SystemSpecification.DatabaseConnectionString}");
        }
        services.AddTransient<IDbContext>((_) => new DbContext(connectionString));

        // tenant management and api controller runtime context
        services.AddScoped<ITenantManager, TenantManager>();
        services.AddScoped<IControllerRuntime, ControllerRuntime>();
        // api query service: singleton to reduce assembly reflection on each query
        services.AddSingleton<IQueryService, QueryService>();

        // repositories
        ApiRepositoryFactory.SetupApiServices(services);
        ApiServiceFactory.SetupApiServices(services, configuration);
    }
}