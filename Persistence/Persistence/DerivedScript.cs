using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence;

/// <summary>Derived script</summary>
public sealed class DerivedScript : Script
{
    /// <summary>The layer level</summary>
    public int Level { get; set; }

    /// <summary>The layer priority</summary>
    public int Priority { get; set; }
}