using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence;

/// <summary>Derived lookup value</summary>
public sealed class DerivedLookupValue : LookupValue
{
    /// <summary>The layer level</summary>
    public int Level { get; set; }

    /// <summary>The layer priority</summary>
    public int Priority { get; set; }
}