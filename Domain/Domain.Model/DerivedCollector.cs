// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.Diagnostics.CodeAnalysis;

namespace PayrollEngine.Domain.Model;

/// <summary>Derived collector</summary>
[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public sealed class DerivedCollector : Collector
{
    /// <summary>Clone this derived collector with empty values</summary>
    public DerivedCollector Clone()
    {
        var clone = new DerivedCollector
        {
            RegulationId = RegulationId,
            Level = Level,
            Priority = Priority
        };
        CopyTool.CopyProperties<Collector>(this, clone);
        return clone;
    }

    /// <summary>The regulation id</summary>
    public int RegulationId { get; set; }

    /// <summary>The layer level</summary>
    public int Level { get; set; }

    /// <summary>The layer priority</summary>
    public int Priority { get; set; }
}