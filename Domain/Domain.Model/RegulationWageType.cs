using System;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A regulation wage type
/// </summary>
public class RegulationWageType : WageType, IEquatable<RegulationWageType>
{
    /// <summary>
    /// The regulationId
    /// </summary>
    public int RegulationId { get; set; }

    /// <inheritdoc/>
    public RegulationWageType()
    {
    }

    /// <inheritdoc/>
    public RegulationWageType(RegulationWageType copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(RegulationWageType compare) =>
        CompareTool.EqualProperties(this, compare);
}