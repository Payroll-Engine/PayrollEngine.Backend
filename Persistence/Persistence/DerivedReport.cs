using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence;

/// <summary>Derived report</summary>
public sealed class DerivedReport : Report
{
    /// <summary>The layer level</summary>
    public int Level { get; set; }

    /// <summary>The layer priority</summary>
    public int Priority { get; set; }
}