using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Represents an issue from the case validation
/// </summary>
public class CaseValidationIssue : IEquatable<CaseValidationIssue>
{
    /// <summary>
    /// The validation issue type
    /// </summary>
    public CaseIssueType IssueType { get; set; }

    /// <summary>
    /// The issue number (negative issue type)
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// Gets the name of the case
    /// </summary>
    public string CaseName { get; set; }

    /// <summary>
    /// The localized case names
    /// </summary>
    public Dictionary<string, string> CaseNameLocalizations { get; set; }

    /// <summary>
    /// The case slot
    /// </summary>
    public string CaseSlot { get; set; }

    /// <summary>
    /// The localized case slot names
    /// </summary>
    public Dictionary<string, string> CaseSlotLocalizations { get; set; }

    /// <summary>
    /// Gets the name of the case field
    /// </summary>
    public string CaseFieldName { get; set; }

    /// <summary>
    /// The localized case field names
    /// </summary>
    public Dictionary<string, string> CaseFieldNameLocalizations { get; set; }

    /// <summary>
    /// The relation source case name
    /// </summary>
    public string SourceCaseName { get; set; }

    /// <summary>
    /// The localized source case names
    /// </summary>
    public Dictionary<string, string> SourceCaseNameLocalizations { get; set; }

    /// <summary>
    /// The relation source case slot
    /// </summary>
    public string SourceCaseSlot { get; set; }

    /// <summary>
    /// The localized source case slots
    /// </summary>
    public Dictionary<string, string> SourceCaseSlotLocalizations { get; set; }

    /// <summary>
    /// The relation target case name
    /// </summary>
    public string TargetCaseName { get; set; }

    /// <summary>
    /// The localized target case names
    /// </summary>
    public Dictionary<string, string> TargetCaseNameLocalizations { get; set; }

    /// <summary>
    /// The relation target case slot
    /// </summary>
    public string TargetCaseSlot { get; set; }

    /// <summary>
    /// The localized target case slots
    /// </summary>
    public Dictionary<string, string> TargetCaseSlotLocalizations { get; set; }

    /// <summary>
    /// Gets the validation message
    /// </summary>
    public string Message { get; set; }

    // TODO: add issue message localizations

    /// <summary>
    /// Initializes a new instance of the <see cref="CaseValidationIssue"/> class
    /// </summary>
    public CaseValidationIssue()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CaseValidationIssue"/> class
    /// </summary>
    /// <param name="copySource">The copy source</param>
    public CaseValidationIssue(CaseValidationIssue copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(CaseValidationIssue compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override string ToString() => Message;
}