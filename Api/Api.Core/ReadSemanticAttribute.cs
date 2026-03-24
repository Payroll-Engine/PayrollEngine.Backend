using System;

namespace PayrollEngine.Api.Core;

/// <summary>
/// Marks a POST action as semantically read-only for tenant isolation purposes.
/// Without this attribute, all POST requests require TenantIsolationLevel.Write.
/// With this attribute, the request is treated as TenantIsolationLevel.Read —
/// allowing it on instances configured with Read or Write isolation.
///
/// Apply to POST actions that do not persist data, for example:
///   - Report execution (execute, execute-query)
///   - Case set build (build-only, no mutation)
///   - Report set retrieval (POST used per RFC 7231 for body-bearing GET semantics)
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class ReadSemanticAttribute : Attribute;
