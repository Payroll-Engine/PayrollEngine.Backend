using System.Collections.Concurrent;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Provides payrun runtime values
/// </summary>
public interface IRuntimeValueProvider
{
    /// <summary>
    /// Payrun runtime values
    /// </summary>
    public ConcurrentDictionary<string, string> PayrunValues { get; }

    /// <summary>
    /// Employees runtime values, key is the employee identifier
    /// </summary>
    public ConcurrentDictionary<string, ConcurrentDictionary<string, string>> EmployeeValues { get; }
}

