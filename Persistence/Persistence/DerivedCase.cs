using PayrollEngine.Domain.Model;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace PayrollEngine.Persistence;

/// <summary>Derived case</summary>
public sealed class DerivedCase : Case
{
    /// <summary>The layer level</summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public int Level { get; set; }

    /// <summary>The layer priority</summary>
    public int Priority { get; set; }
}