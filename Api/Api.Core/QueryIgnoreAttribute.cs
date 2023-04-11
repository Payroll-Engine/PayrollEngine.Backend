using System;

namespace PayrollEngine.Api.Core;

/// <summary>
/// Marks a non query method
/// </summary>
/// <seealso cref="Attribute" />
[AttributeUsage(AttributeTargets.Method)]
public sealed class QueryIgnoreAttribute : Attribute
{
}