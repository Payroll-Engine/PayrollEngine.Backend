using System.Collections.Generic;

namespace PayrollEngine.Domain.Scripting;

/// <summary>
/// Provides payrun runtime values
/// </summary>
public sealed class RuntimeValueProvider
{
    /// <summary>
    /// Payrun runtime values
    /// </summary>
    public Dictionary<string, string> PayrunValues { get; } = new();

    /// <summary>
    /// Employees runtime values, key is the employee identifier
    /// </summary>
    public Dictionary<string, Dictionary<string, string>> EmployeeValues { get; } = new();
}