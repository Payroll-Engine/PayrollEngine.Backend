using System.Collections.Generic;
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A payroll collector audit
/// </summary>
public class CollectorAudit : ScriptAuditDomainObject, IDomainAttributeObject
{
    /// <summary>
    /// The collector id
    /// </summary>
    public int CollectorId { get; set; }

    /// <summary>
    /// The collector name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The localized collector names
    /// </summary>
    public Dictionary<string, string> NameLocalizations { get; set; }

    /// <summary>
    /// The collect mode
    /// </summary>
    public CollectMode CollectMode { get; set; }

    /// <summary>
    /// Negated collector result
    /// </summary>
    public bool Negated { get; set; }

    /// <summary>
    /// The override type
    /// </summary>
    public OverrideType OverrideType { get; set; }

    /// <summary>
    /// The value type
    /// </summary>
    public ValueType ValueType { get; set; }

    /// <summary>
    /// The collector culture name based on RFC 4646
    /// </summary>
    public string Culture { get; set; }

    /// <summary>
    /// Associated collector groups
    /// </summary>
    public List<string> CollectorGroups { get; set; }

    /// <summary>
    /// The threshold value
    /// </summary>
    public decimal? Threshold { get; set; }

    /// <summary>
    /// The minimum allowed value
    /// </summary>
    public decimal? MinResult { get; set; }

    /// <summary>
    /// The maximum allowed value
    /// </summary>
    public decimal? MaxResult { get; set; }

    /// <summary>
    /// Expression used while the collector is started
    /// </summary>
    public string StartExpression { get; set; }

    /// <summary>
    /// Expression used while applying a value to the collector
    /// </summary>
    public string ApplyExpression { get; set; }

    /// <summary>
    /// Expression used while the collector is ended
    /// </summary>
    public string EndExpression { get; set; }
    /// <summary>
    /// The collector start actions
    /// </summary>
    public List<string> StartActions { get; set; }

    /// <summary>
    /// The collector apply actions
    /// </summary>
    public List<string> ApplyActions { get; set; }

    /// <summary>
    /// The collector end actions
    /// </summary>
    public List<string> EndActions { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <summary>
    /// The collector clusters
    /// </summary>
    public List<string> Clusters { get; set; }

    /// <inheritdoc/>
    public CollectorAudit()
    {
    }

    /// <inheritdoc/>
    public CollectorAudit(CollectorAudit copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Name} {base.ToString()}";
}