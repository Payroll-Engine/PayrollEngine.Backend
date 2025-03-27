using System;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Case relation reference
/// </summary>
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
public class CaseRelationReference : IEquatable<CaseRelationReference>
{
    /// <summary>
    /// The relation source case name (immutable)
    /// </summary>
    public string SourceCaseName { get; set; }

    /// <summary>
    /// The relation source case slot
    /// </summary>
    public string SourceCaseSlot { get; set; }

    /// <summary>
    /// The relation target case name (immutable)
    /// </summary>
    public string TargetCaseName { get; set; }

    /// <summary>
    /// The relation target case slot
    /// </summary>
    public string TargetCaseSlot { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public CaseRelationReference()
    {
    }

    /// <summary>
    /// Case constructor
    /// </summary>
    /// <param name="sourceCaseName">The relation source case name</param>
    /// <param name="targetCaseName">The relation target case name</param>
    public CaseRelationReference(string sourceCaseName, string targetCaseName)
    {
        SourceCaseName = sourceCaseName;
        TargetCaseName = targetCaseName;
    }

    /// <summary>
    /// Case with slot constructor
    /// </summary>
    /// <param name="sourceCaseName">The relation source case name</param>
    /// <param name="sourceCaseSlot">The relation source case slot</param>
    /// <param name="targetCaseName">The relation target case name</param>
    /// <param name="targetCaseSlot">The relation target case slot</param>
    public CaseRelationReference(string sourceCaseName, string sourceCaseSlot,
        string targetCaseName, string targetCaseSlot)
    {
        SourceCaseName = sourceCaseName;
        SourceCaseSlot =sourceCaseSlot;
        TargetCaseName = targetCaseName;
        TargetCaseSlot = targetCaseSlot;
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(CaseRelationReference compare) =>
        CompareTool.EqualProperties(this, compare);
}