using System;

namespace PayrollEngine.Api.Core;

/// <summary>
/// Marks a controller or action as a cross-tenant endpoint.
/// Cross-tenant endpoints operate outside any single tenant scope and
/// therefore require at least <see cref="TenantIsolationLevel.Read"/>
/// isolation — even when the server is configured with
/// <see cref="TenantIsolationLevel.None"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class CrossTenantEndpointAttribute : Attribute;
