using System;

namespace PayrollEngine.Api.Core;

/// <summary>Mark a non query method</summary>
/// <seealso cref="Attribute" />
[AttributeUsage(AttributeTargets.Method)]
public sealed class QueryIgnoreAttribute : Attribute
{
}