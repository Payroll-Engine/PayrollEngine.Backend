using System;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Child case field
/// </summary>
public class ChildCaseField : CaseField, IEquatable<ChildCaseField>
{
    /// <summary>
    /// The case id
    /// </summary>
    public int CaseId { get; set; }

    /// <summary>
    /// The regulation id
    /// </summary>
    public int RegulationId { get; set; }

    /// <summary>
    /// The case type
    /// </summary>
    public CaseType CaseType { get; set; }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(ChildCaseField compare) =>
        CompareTool.EqualProperties(this, compare);
}