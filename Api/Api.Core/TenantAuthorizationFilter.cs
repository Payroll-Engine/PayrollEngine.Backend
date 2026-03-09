using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PayrollEngine.Api.Core;

/// <summary>
/// Action filter that performs tenant authorization.
/// Extracts the tenant id from the route, validates the Auth-Tenant header,
/// and applies the tenant culture to the current async context.
/// <para>Skipped when the action is decorated with <see cref="SkipTenantAuthAttribute"/>.</para>
/// </summary>
public class TenantAuthorizationFilter(IControllerRuntime runtime) : IAsyncActionFilter
{
    /// <inheritdoc />
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // skip when action is decorated with [SkipTenantAuth]
        if (context.ActionDescriptor.EndpointMetadata.OfType<SkipTenantAuthAttribute>().Any())
        {
            await next();
            return;
        }

        // resolve tenantId from route or action arguments
        if (!TryGetTenantId(context, out var tenantId))
        {
            await next();
            return;
        }

        // tenant authorization header
        context.HttpContext.Request.Headers
            .TryGetValue(BackendSpecification.TenantAuthorizationHeader, out var authTenant);

        // validate tenant
        var tenant = await runtime.DbContext.GetTenantAsync(tenantId, authTenant);
        if (tenant == null)
        {
            context.Result = new UnauthorizedObjectResult($"Invalid tenant with id {tenantId}");
            return;
        }

        // apply tenant culture to the current async context
        if (!string.IsNullOrWhiteSpace(tenant.Culture) &&
            !string.Equals(CultureInfo.CurrentCulture.Name, tenant.Culture))
        {
            CultureInfo.CurrentCulture = new(tenant.Culture);
        }

        await next();
    }

    private static bool TryGetTenantId(ActionExecutingContext context, out int tenantId)
    {
        tenantId = 0;

        // try route values first
        if (context.RouteData.Values.TryGetValue("tenantId", out var routeValue))
        {
            return routeValue is string routeString && int.TryParse(routeString, out tenantId);
        }

        // try action arguments (for cases where tenantId is a method parameter)
        if (context.ActionArguments.TryGetValue("tenantId", out var argValue) && argValue is int id)
        {
            tenantId = id;
            return true;
        }

        return false;
    }
}
