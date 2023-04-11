using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The regulation case audit API object (immutable)
/// </summary>
public class CaseAudit : ApiObjectBase
{
    /// <summary>
    /// The case id
    /// </summary>
    [Required]
    public int CaseId { get; set; }

    /// <summary>
    /// The type of he case (immutable)
    /// </summary>
    [Required]
    public CaseType CaseType { get; set; }

    /// <summary>
    /// The case name (immutable)
    /// </summary>
    [Required]
    [StringLength(128)]
    public string Name { get; set; }

    /// <summary>
    /// The localized case names
    /// </summary>
    [Localization(nameof(Name))]
    public Dictionary<string, string> NameLocalizations { get; set; }

    /// <summary>
    /// The case description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The localized case descriptions
    /// </summary>
    [Localization(nameof(Description))]
    public Dictionary<string, string> DescriptionLocalizations { get; set; }

    /// <summary>
    /// The override type
    /// </summary>
    public OverrideType OverrideType { get; set; }

    /// <summary>
    /// The cancellation type
    /// </summary>
    public CaseCancellationType CancellationType { get; set; }

    /// <summary>
    /// The base case name
    /// </summary>
    [StringLength(128)]
    public string BaseCase { get; set; }
        
    /// <summary>
    /// The base case fields
    /// </summary>
    public CaseFieldReference[] BaseCaseFields { get; set; }

    /// <summary>
    /// The expression used to build a case
    /// </summary>
    public string AvailableExpression { get; set; }

    /// <summary>
    /// The expression used to build a case
    /// </summary>
    public string BuildExpression { get; set; }

    /// <summary>
    /// The case validate expression
    /// </summary>
    public string ValidateExpression { get; set; }

    /// <summary>
    /// The lookups
    /// </summary>
    public List<string> Lookups { get; set; }

    /// <summary>
    /// The case slots
    /// </summary>
    public List<CaseSlot> Slots { get; set; }

    /// <summary>
    /// The case build actions
    /// </summary>
    public List<string> BuildActions { get; set; }

    /// <summary>
    /// The case validate actions
    /// </summary>
    public List<string> ValidateActions { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <summary>
    /// The case clusters
    /// </summary>
    public List<string> Clusters { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Name} {base.ToString()}";
}