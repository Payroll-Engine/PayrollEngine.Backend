using System.Collections.Concurrent;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Domain.Scripting;

/// <summary>
/// Provides payrun runtime values
/// </summary>
public sealed class RuntimeValueProvider : IRuntimeValueProvider
{
    /// <inheritdoc />
    public ConcurrentDictionary<string, string> PayrunValues { get; } = new();

    /// <inheritdoc />
    public ConcurrentDictionary<string, ConcurrentDictionary<string, string>> EmployeeValues { get; } = new();
}