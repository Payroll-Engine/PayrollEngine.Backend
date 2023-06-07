using PayrollEngine.Domain.Model;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Scripting;

/// <summary>
/// Provides payrun runtime values
/// </summary>
public sealed class RuntimeValueProvider : IRuntimeValueProvider
{
    /// <inheritdoc />
    public Dictionary<string, string> PayrunValues { get; } = new();

    /// <inheritdoc />
    public Dictionary<string, Dictionary<string, string>> EmployeeValues { get; } = new();
}