using System;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A regulation case
/// </summary>
public class RegulationCase : Case, IEquatable<RegulationCase>
{
    /// <summary>
    /// The regulationId
    /// </summary>
    public int RegulationId { get; set; }

    /// <inheritdoc/>
    public RegulationCase()
    {
    }

    /// <inheritdoc/>
    public RegulationCase(RegulationCase copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(RegulationCase compare) =>
        CompareTool.EqualProperties(this, compare);
}