using PayrollEngine.Domain.Model;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace PayrollEngine.Persistence;

/// <summary>Derived collector</summary>
public sealed class DerivedCollector : Collector
{
    /// <summary>The layer level</summary>
    public int Level { get; set; }

    /// <summary>The layer priority</summary>
    public int Priority { get; set; }
}