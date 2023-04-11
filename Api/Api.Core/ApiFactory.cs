using PayrollEngine.Domain.Scripting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PayrollEngine.Persistence;

namespace PayrollEngine.Api.Core;

internal static class ApiFactory
{
    // services setup
    internal static void SetupApiServices(IServiceCollection services, IConfiguration configuration, IDbContext context)
    {
        // tenant management and api controller runtime context
        services.AddScoped<ITenantManager, TenantManager>();
        services.AddScoped<IControllerRuntime, ControllerRuntime>();
        // api query service: singleton to reduce assembly reflection on each query
        services.AddSingleton<IQueryService, QueryService>();

        // repositories
        ApiRepositoryFactory.SetupApiServices(services, context);
        ApiServiceFactory.SetupApiServices(services, context);
    }
}