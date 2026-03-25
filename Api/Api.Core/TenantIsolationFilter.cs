using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace PayrollEngine.Api.Core;

/// <summary>
/// Global action filter enforcing the server-wide TenantIsolationLevel policy.
/// Runs before TenantAuthorizationFilter on every request.
///
/// The filter is only active for cross-tenant HTTP access (Read / Write level).
/// In single-tenant mode (None or Consolidation) the filter is fully transparent —
/// normal clients (PE Console, WebApp) that do not send the Auth-Tenant header are
/// never blocked.
///
/// Rules (evaluated in order):
///   1. [SkipTenantAuth] on the action → skip all checks.
///
///   2. configuredLevel &lt; Read (None or Consolidation) → single-tenant HTTP mode → always permitted.
///      Cross-tenant HTTP is not enabled; the filter does not restrict anything.
///      (Consolidation only enables ExecuteConsolidatedQuery in report scripts, not HTTP.)
///
///   3. Auth-Tenant header present AND TenantIsolationLevel == Write → 400 Bad Request.
///      Write-level implies unrestricted multi-tenant access; the scoping header contradicts that.
///
///   4. Auth-Tenant header present → single-tenant scoped request → always permitted.
///
///   5. Auth-Tenant header absent → cross-tenant HTTP request → level check:
///      - GET / HEAD                   → requires Read
///      - POST [ReadSemantic]          → requires Read
///      - POST / PUT / PATCH / DELETE  → requires Write
///      If configuredLevel &lt; required → 403 Forbidden.
/// </summary>
public class TenantIsolationFilter(IOptions<PayrollServerConfiguration> config) : IAsyncActionFilter
{
    private readonly TenantIsolationLevel configuredLevel = config.Value.TenantIsolationLevel;

    /// <inheritdoc />
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // rule 1: skip when action opts out of tenant checks entirely
        if (context.ActionDescriptor.EndpointMetadata.OfType<SkipTenantAuthAttribute>().Any())
        {
            await next();
            return;
        }

        // rule 2: single-tenant HTTP mode → filter is transparent, UNLESS the endpoint is
        // explicitly marked as cross-tenant. Cross-tenant endpoints (e.g. /api/shares/regulations)
        // operate outside any single tenant scope and therefore always require at least Read level.
        var isCrossTenantEndpoint = context.ActionDescriptor.EndpointMetadata
            .OfType<CrossTenantEndpointAttribute>().Any();
        if (configuredLevel < TenantIsolationLevel.Read && !isCrossTenantEndpoint)
        {
            await next();
            return;
        }

        var method = context.HttpContext.Request.Method;
        var hasAuthTenantHeader = context.HttpContext.Request.Headers
            .ContainsKey(BackendSpecification.TenantAuthorizationHeader);

        // rule 3: Write-level + Auth-Tenant header = contradictory configuration → 400
        if (configuredLevel == TenantIsolationLevel.Write && hasAuthTenantHeader)
        {
            context.Result = new BadRequestObjectResult(
                $"The '{BackendSpecification.TenantAuthorizationHeader}' header must not be sent " +
                $"when the server is configured with TenantIsolationLevel.Write. " +
                $"Write-level implies unrestricted multi-tenant access.");
            return;
        }

        // rule 4: Auth-Tenant header present → single-tenant scoped request → always allowed
        if (hasAuthTenantHeader)
        {
            await next();
            return;
        }

        // rule 5: no Auth-Tenant header, configuredLevel >= Read → cross-tenant HTTP → enforce level
        var required = GetRequiredLevel(context, method);
        if (configuredLevel < required)
        {
            context.Result = new ObjectResult(
                $"Cross-tenant access denied. Required isolation level: {required}, " +
                $"configured: {configuredLevel}.")
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
            return;
        }

        await next();
    }

    /// <summary>Determines the required TenantIsolationLevel for an unscoped (no Auth-Tenant) request.</summary>
    private static TenantIsolationLevel GetRequiredLevel(ActionExecutingContext context, string method)
    {
        if (HttpMethods.IsGet(method) || HttpMethods.IsHead(method))
            return TenantIsolationLevel.Read;

        if (HttpMethods.IsPost(method))
        {
            var isReadSemantic = context.ActionDescriptor.EndpointMetadata
                .OfType<ReadSemanticAttribute>().Any();
            return isReadSemantic ? TenantIsolationLevel.Read : TenantIsolationLevel.Write;
        }

        // PUT, PATCH, DELETE
        return TenantIsolationLevel.Write;
    }
}
