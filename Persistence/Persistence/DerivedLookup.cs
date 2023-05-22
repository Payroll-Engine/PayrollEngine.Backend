using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence;

/// <summary>Derived lookup</summary>
public sealed class DerivedLookup : Lookup
{
    /// <summary>The layer level</summary>
    public int Level { get; set; }

    /// <summary>The layer priority</summary>
    public int Priority { get; set; }
}