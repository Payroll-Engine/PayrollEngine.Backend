// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace PayrollEngine.Domain.Model;

/// <summary>Derived lookup value</summary>
public sealed class DerivedLookupValue : LookupValue
{
    /// <summary>The regulation id</summary>
    public int RegulationId { get; set; }

    /// <summary>The layer level</summary>
    public int Level { get; set; }

    /// <summary>The layer priority</summary>
    public int Priority { get; set; }
}