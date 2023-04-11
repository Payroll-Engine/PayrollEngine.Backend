using System;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A regulation case field
/// </summary>
public class RegulationCaseField : CaseField, IEquatable<RegulationCaseField>
{
    /// <summary>
    /// The regulationId
    /// </summary>
    public int RegulationId { get; set; }

    /// <summary>
    /// The case id
    /// </summary>
    public int CaseId { get; set; }

    /// <summary>
    /// The case type
    /// </summary>
    public CaseType CaseType { get; set; }

    /// <inheritdoc/>
    public RegulationCaseField()
    {
    }

    /// <inheritdoc/>
    public RegulationCaseField(RegulationCaseField copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(RegulationCaseField compare) =>
        CompareTool.EqualProperties(this, compare);
}