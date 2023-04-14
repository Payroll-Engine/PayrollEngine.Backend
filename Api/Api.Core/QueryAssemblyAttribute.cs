using System;

namespace PayrollEngine.Api.Core;

/// <summary>Mark a query assembly</summary>
/// <seealso cref="QueryIgnoreAttribute" />
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class QueryAssemblyAttribute : Attribute
{
}