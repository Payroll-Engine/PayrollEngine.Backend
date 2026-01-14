using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A case field used in national, company and employee case
/// </summary>
public class CaseField : TrackDomainObject<CaseFieldAudit>, IDerivableObject, IClusterObject,
    INamedObject, INamespaceObject, IDomainAttributeObject, IEquatable<CaseField>
{
    /// <summary>
    /// The case field name (immutable)
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The localized case field names
    /// </summary>
    public Dictionary<string, string> NameLocalizations { get; set; }

    /// <summary>
    /// The case field description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The localized case field descriptions
    /// </summary>
    public Dictionary<string, string> DescriptionLocalizations { get; set; }

    /// <summary>
    /// The value type of the case field
    /// </summary>
    public ValueType ValueType { get; set; }

    /// <summary>
    /// The value scope
    /// </summary>
    public ValueScope ValueScope { get; set; }

    /// <summary>
    /// The date period type
    /// </summary>
    public CaseFieldTimeType TimeType { get; set; }

    /// <summary>
    /// The date unit type
    /// </summary>
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
    /// The case value creation mode
    /// </summary>
    public CaseValueCreationMode ValueCreationMode { get; set; }

    /// <summary>
    /// The case field culture name based on RFC 4646
    /// </summary>
    public string Culture { get; set; }

    /// <summary>
    /// Mandatory case field value
    /// </summary>
    public bool ValueMandatory { get; set; }

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
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <summary>
    /// Custom value attributes
    /// </summary>
    public Dictionary<string, object> ValueAttributes { get; set; }

    /// <inheritdoc/>
    public CaseField()
    {
    }

    /// <inheritdoc/>
    protected CaseField(CaseField copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <inheritdoc/>
    public void ApplyNamespace(string @namespace)
    {
        Name = Name.EnsureNamespace(@namespace);
        if (LookupSettings != null)
        {
            LookupSettings.LookupName = LookupSettings.LookupName.EnsureNamespace(@namespace);
        }
        Clusters = Clusters.EnsureNamespace(@namespace);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(CaseField compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override CaseFieldAudit ToAuditObject()
    {
        if (!this.HasId())
        {
            return null;
        }
        return new()
        {
            CaseFieldId = Id,
            Name = Name,
            NameLocalizations = NameLocalizations,
            Description = Description,
            DescriptionLocalizations = DescriptionLocalizations,
            ValueType = ValueType,
            ValueScope = ValueScope,
            TimeType = TimeType,
            TimeUnit = TimeUnit,
            OverrideType = OverrideType,
            CancellationMode = CancellationMode,
            ValueCreationMode = ValueCreationMode,
            Culture = Culture,
            ValueMandatory = ValueMandatory,
            Order = Order,
            StartDateType = StartDateType,
            EndDateType = EndDateType,
            EndMandatory = EndMandatory,
            DefaultStart = DefaultStart,
            DefaultEnd = DefaultEnd,
            DefaultValue = DefaultValue,
            Tags = Tags,
            LookupSettings = LookupSettings != null ? new LookupSettings(LookupSettings) : null,
            Clusters = Clusters,
            Attributes = Attributes,
            ValueAttributes = ValueAttributes
        };
    }

    /// <inheritdoc/>
    public override void FromAuditObject(CaseFieldAudit audit)
    {
        // base values
        base.FromAuditObject(audit);

        // copy values from audit object
        Id = audit.CaseFieldId;
        Name = audit.Name;
        NameLocalizations = audit.NameLocalizations;
        Description = audit.Description;
        DescriptionLocalizations = audit.DescriptionLocalizations;
        // base values
        ValueType = audit.ValueType;
        ValueScope = audit.ValueScope;
        TimeType = audit.TimeType;
        TimeUnit = audit.TimeUnit;
        OverrideType = audit.OverrideType;
        CancellationMode = audit.CancellationMode;
        ValueCreationMode = audit.ValueCreationMode;
        Culture = audit.Culture;
        ValueMandatory = audit.ValueMandatory;
        Order = audit.Order;
        StartDateType = audit.StartDateType;
        EndDateType = audit.EndDateType;
        EndMandatory = audit.EndMandatory;
        DefaultStart = audit.DefaultStart;
        DefaultEnd = audit.DefaultEnd;
        DefaultValue = audit.DefaultValue;
        // collections
        Tags = audit.Tags;
        LookupSettings = audit.LookupSettings;
        Clusters = audit.Clusters;
        Attributes = audit.Attributes;
        ValueAttributes = audit.ValueAttributes;
    }

    /// <inheritdoc/>
    public override string ToString() =>
        string.IsNullOrWhiteSpace(DefaultValue) ? $" {Name} {base.ToString()}" :
            $" {Name}={DefaultValue} {base.ToString()}";
}