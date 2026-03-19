using System;
using Microsoft.AspNetCore.Mvc;

namespace PayrollEngine.Api.Core;

/// <summary>
/// Performs tenant authorization by validating the tenant id from the route
/// against the Auth-Tenant header, and applies the tenant culture to the request.
/// <para>Can be applied at controller level (all actions) or action level.
/// Use <see cref="SkipTenantAuthAttribute"/> to opt-out individual actions.</para>
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class TenantAuthorizeAttribute : TypeFilterAttribute
{
    /// <inheritdoc />
    public TenantAuthorizeAttribute() : base(typeof(TenantAuthorizationFilter))
    {
    }
}
