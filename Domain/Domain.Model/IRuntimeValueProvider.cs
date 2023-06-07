using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Provides payrun runtime values
/// </summary>
public interface IRuntimeValueProvider
{
    /// <summary>
    /// Payrun runtime values
    /// </summary>
    public Dictionary<string, string> PayrunValues { get; }

    /// <summary>
    /// Employees runtime values, key is the employee identifier
    /// </summary>
    public Dictionary<string, Dictionary<string, string>> EmployeeValues { get; }
}

