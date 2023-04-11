using System;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A regulation collector
/// </summary>
public class RegulationCollector : Collector, IEquatable<RegulationCollector>
{
    /// <summary>
    /// The regulationId
    /// </summary>
    public int RegulationId { get; set; }

    /// <inheritdoc/>
    public RegulationCollector()
    {
    }

    /// <inheritdoc/>
    public RegulationCollector(RegulationCollector copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(RegulationCollector compare) =>
        CompareTool.EqualProperties(this, compare);
}