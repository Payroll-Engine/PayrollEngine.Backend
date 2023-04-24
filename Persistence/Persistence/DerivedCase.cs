using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence;

/// <summary>Derived case</summary>
public sealed class DerivedCase : Case
{
    /// <summary>The layer level</summary>
    public int Level { get; set; }

    /// <summary>The layer priority</summary>
    public int Priority { get; set; }
}