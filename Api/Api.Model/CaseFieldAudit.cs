using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace PayrollEngine.Api.Model;

/// <summary>
/// The regulation case field audit API object (immutable)
/// </summary>
public class CaseFieldAudit : ApiObjectBase
{
    /// <summary>
    /// The case field id
    /// </summary>
    [Required]
    public int CaseFieldId { get; set; }

    /// <summary>
    /// The case field name
    /// </summary>
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// The localized case field names
    /// </summary>
    [Localization(nameof(Name))]
    public Dictionary<string, string> NameLocalizations { get; set; }

    /// <summary>
    /// The case field description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The localized case field descriptions
    /// </summary>
    [Localization(nameof(Description))]
    public Dictionary<string, string> DescriptionLocalizations { get; set; }

    /// <summary>
    /// The value type of the case field
    /// </summary>
    [Required]
    public ValueType ValueType { get; set; }

    /// <summary>
    /// The value scope
    /// </summary>
    public ValueScope ValueScope { get; set; }

    /// <summary>
    /// The case field time type
    /// </summary>
    [Required]
    public CaseFieldTimeType TimeType { get; set; }

    /// <summary>
    /// The date unit type
    /// </summary>
    [Required]
    public CaseFieldTimeUnit TimeUnit { get; set; }

    /// <summary>
    /// The period aggregation type
    /// </summary>
    public CaseFieldAggregationType PeriodAggregation { get; set; }

    /// <summary>
    /// The override type
    /// </summary>
    public OverrideType OverrideType { get; set; }

    /// <summary>
    /// The cancellation mode
    /// </summary>
    public CaseFieldCancellationMode CancellationMode { get; set; }

    /// <summary>
    /// The case field culture name based on RFC 4646
    /// </summary>
    public string Culture { get; set; }

    /// <summary>
    /// Mandatory case field
    /// </summary>
    [Required]
    public bool Mandatory { get; set; }

    /// <summary>
    /// The case field order
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// The start date type
    /// </summary>
    public CaseFieldDateType StartDateType { get; set; }

    /// <summary>
    /// The end date type
    /// </summary>
    public CaseFieldDateType EndDateType { get; set; }

    /// <summary>
    /// The end date mandatory state
    /// </summary>
    public bool EndMandatory { get; set; }

    /// <summary>
    /// The default start value of the case field (date or expression)
    /// </summary>
    public string DefaultStart { get; set; }

    /// <summary>
    /// The default end value of the case field (date or expression)
    /// </summary>
    public string DefaultEnd { get; set; }

    /// <summary>
    /// The default value of the case field (JSON format)
    /// </summary>
    public string DefaultValue { get; set; }

    /// <summary>
    /// The case field tags
    /// </summary>
    public List<string> Tags { get; set; }

    /// <summary>
    /// The lookup settings
    /// </summary>
    public LookupSettings LookupSettings { get; set; }

    /// <summary>
    /// The case field clusters
    /// </summary>
    public List<string> Clusters { get; set; }

    /// <summary>
    /// The case field build actions
    /// </summary>
    public List<string> BuildActions { get; set; }

    /// <summary>
    /// The case field validate actions
    /// </summary>
    public List<string> ValidateActions { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <summary>
    /// Custom value attributes
    /// </summary>
    public Dictionary<string, object> ValueAttributes { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{ValueType} {base.ToString()}";
}