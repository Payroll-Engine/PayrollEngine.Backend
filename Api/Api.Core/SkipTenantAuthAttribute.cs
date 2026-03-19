using System;

namespace PayrollEngine.Api.Core;

/// <summary>
/// Marks an action to skip tenant authorization when the controller
/// is decorated with <see cref="TenantAuthorizeAttribute"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class SkipTenantAuthAttribute : Attribute;
