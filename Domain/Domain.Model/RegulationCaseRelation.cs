using System;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A regulation case relation
/// </summary>
public class RegulationCaseRelation : CaseRelation, IEquatable<RegulationCaseRelation>
{
    /// <summary>
    /// The regulationId
    /// </summary>
    public int RegulationId { get; set; }

    /// <inheritdoc/>
    public RegulationCaseRelation()
    {
    }

    /// <inheritdoc/>
    public RegulationCaseRelation(RegulationCaseRelation copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(RegulationCaseRelation compare) =>
        CompareTool.EqualProperties(this, compare);
}