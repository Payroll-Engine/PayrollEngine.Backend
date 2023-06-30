using System.Collections.Generic;
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A payroll case audit
/// </summary>
public class CaseAudit : ScriptAuditDomainObject, IDomainAttributeObject
{
    /// <summary>
    /// The case id
    /// </summary>
    public int CaseId { get; set; }

    /// <summary>
    /// The type of he case (immutable)
    /// </summary>
    public CaseType CaseType { get; set; }

    /// <summary>
    /// The case name (immutable)
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The localized case names
    /// </summary>
    public Dictionary<string, string> NameLocalizations { get; set; }

    /// <summary>
    /// Synonyms for the case name
    /// </summary>
    public List<string> NameSynonyms { get; set; }

    /// <summary>
    /// The case description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The localized case descriptions
    /// </summary>
    public Dictionary<string, string> DescriptionLocalizations { get; set; }

    /// <summary>
    /// The base case name
    /// </summary>
    public string BaseCase { get; set; }

    /// <summary>
    /// The base case fields
    /// </summary>
    public List<CaseFieldReference> BaseCaseFields { get; set; }

    /// <summary>
    /// The override type
    /// </summary>
    public OverrideType OverrideType { get; set; }

    /// <summary>
    /// The cancellation type
    /// </summary>
    public CaseCancellationType CancellationType { get; set; }

    /// <summary>
    /// The default case change reason
    /// </summary>
    public string DefaultReason { get; set; }

    /// <summary>
    /// The localized default case change reasons
    /// </summary>
    public Dictionary<string, string> DefaultReasonLocalizations { get; set; }

    /// <summary>
    /// The expression used to build a case
    /// </summary>
    public string AvailableExpression { get; set; }

    /// <summary>
    /// The expression used to build a case
    /// </summary>
    public string BuildExpression { get; set; }

    /// <summary>
    /// The expression which evaluates if the case is valid
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
    /// The case available actions
    /// </summary>
    public List<string> AvailableActions { get; set; }

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
    public CaseAudit()
    {
    }

    /// <inheritdoc/>
    public CaseAudit(CaseAudit copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Name} {base.ToString()}";
}