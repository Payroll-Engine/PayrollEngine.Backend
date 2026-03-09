using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using PayrollEngine.Backend.Controller;
using PayrollEngine.Domain.Application;
using PayrollEngine.Domain.Application.Service;

namespace PayrollEngine.Backend.Server;

/// <summary>
/// Startup extension
/// </summary>
public static class ProviderStartupExtensions
{
    /// <summary>
    /// Adds the local API services
    /// </summary>
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IServiceCollection AddLocalApiServices(this IServiceCollection services)
    {
        // service mappings
        services.AddTransient<ITenantService, TenantService>();

        // register all concrete controllers from the backend controller assembly
        var controllerTypes = typeof(UserController).Assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsClass: true } &&
                        t.IsAssignableTo(typeof(ControllerBase)));

        foreach (var type in controllerTypes)
        {
            services.AddTransient(type);
        }

        return services;
    }
}