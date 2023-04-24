using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence;

/// <summary>Derived wage type</summary>
public sealed class DerivedWageType : WageType
{
    /// <summary>The layer level</summary>
    public int Level { get; set; }

    /// <summary>The layer priority</summary>
    public int Priority { get; set; }
}