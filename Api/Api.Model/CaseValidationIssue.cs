using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// Represents an issue from the case validation
/// </summary>
public class CaseValidationIssue
{
    /// <summary>
    /// The validation issue type
    /// </summary>
    [Required]
    public CaseIssueType IssueType { get; set; }

    /// <summary>
    /// The issue number (negative issue type)
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// Gets the name of the case
    /// </summary>
    [Required]
    [StringLength(128)]
    public string CaseName { get; set; }

    /// <summary>
    /// The localized case names
    /// </summary>
    public Dictionary<string, string> CaseNameLocalizations { get; set; }

    /// <summary>
    /// The case slot
    /// </summary>
    [StringLength(128)]
    public string CaseSlot { get; set; }

    /// <summary>
    /// The localized case slot names
    /// </summary>
    public Dictionary<string, string> CaseSlotLocalizations { get; set; }

    /// <summary>
    /// Gets the name of the case field
    /// </summary>
    [StringLength(128)]
    public string CaseFieldName { get; set; }

    /// <summary>
    /// The localized case field names
    /// </summary>
    public Dictionary<string, string> CaseFieldNameLocalizations { get; set; }

    /// <summary>
    /// The relation source case name
    /// </summary>
    [StringLength(128)]
    public string SourceCaseName { get; set; }

    /// <summary>
    /// The localized source case names
    /// </summary>
    public Dictionary<string, string> SourceCaseNameLocalizations { get; set; }

    /// <summary>
    /// The relation source case slot
    /// </summary>
    [StringLength(128)]
    public string SourceCaseSlot { get; set; }

    /// <summary>
    /// The localized source case slots
    /// </summary>
    public Dictionary<string, string> SourceCaseSlotLocalizations { get; set; }

    /// <summary>
    /// The relation target case name
    /// </summary>
    [StringLength(128)]
    public string TargetCaseName { get; set; }

    /// <summary>
    /// The localized target case names
    /// </summary>
    public Dictionary<string, string> TargetCaseNameLocalizations { get; set; }

    /// <summary>
    /// The relation target case slot
    /// </summary>
    [StringLength(128)]
    public string TargetCaseSlot { get; set; }

    /// <summary>
    /// The localized target case slots
    /// </summary>
    public Dictionary<string, string> TargetCaseSlotLocalizations { get; set; }

    /// <summary>
    /// Gets the validation message
    /// </summary>
    [Required]
    public string Message { get; set; }

    /// <inheritdoc/>
    public override string ToString() => Message;
}