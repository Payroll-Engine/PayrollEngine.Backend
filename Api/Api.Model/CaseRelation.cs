using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace PayrollEngine.Api.Model;

/// <summary>
/// The regulation case relation API object
/// </summary>
public class CaseRelation : ApiObjectBase
{
    /// <summary>
    /// The relation source case name (immutable)
    /// </summary>
    [Required]
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
    /// The relation target case name (immutable)
    /// </summary>
    [Required]
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
    /// The expression used to build the case relation
    /// </summary>
    public string BuildExpression { get; set; }

    /// <summary>
    /// The expression which evaluates if the case relation is valid
    /// </summary>
    public string ValidateExpression { get; set; }

    /// <summary>
    /// The override type
    /// </summary>
    public OverrideType OverrideType { get; set; }

    /// <summary>
    /// The case relation order
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// The case relation build actions
    /// </summary>
    public List<string> BuildActions { get; set; }

    /// <summary>
    /// The case relation validate actions
    /// </summary>
    public List<string> ValidateActions { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <summary>
    /// The case relation clusters
    /// </summary>
    public List<string> Clusters { get; set; }
}