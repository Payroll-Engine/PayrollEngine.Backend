using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Represents a value within a lookup
/// </summary>
public class LookupValue : TrackDomainObject<LookupValueAudit>, INamespaceObject, IDerivableObject, IEquatable<LookupValue>
{
    /// <summary>
    /// The lookup value key
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// The lookup key hash code
    /// The hash is used by database indexes
    /// </summary>
    public int KeyHash
    {
        get => Key.ToPayrollHash();
        // ReSharper disable once ValueParameterNotUsed
        set
        {
            // hash is calculated
        }
    }

    /// <summary>
    /// The lookup range value
    /// </summary>
    public decimal? RangeValue { get; set; }

    /// <summary>
    /// The lookup value as JSON
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// The localized lookup values
    /// </summary>
    public Dictionary<string, string> ValueLocalizations { get; set; }

    /// <summary>
    /// The lookup hash code: combined key with range value
    /// The hash is used by database indexes
    /// </summary>
    public int LookupHash
    {
        get => Key.ToPayrollHash(RangeValue);
        // ReSharper disable once ValueParameterNotUsed
        set
        {
            // hash is calculated
        }
    }

    /// <summary>
    /// The override type
    /// </summary>
    public OverrideType OverrideType { get; set; }

    /// <inheritdoc/>
    public LookupValue()
    {
    }

    /// <inheritdoc/>
    public LookupValue(LookupValue copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }
    
    /// <inheritdoc/>
    public void ApplyNamespace(string @namespace)
    {
        // no namespace fields
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(LookupValue compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override LookupValueAudit ToAuditObject()
    {
        if (!this.HasId())
        {
            return null;
        }
        return new()
        {
            LookupValueId = Id,
            Key = Key,
            KeyHash = KeyHash,
            RangeValue = RangeValue,
            Value = Value,
            ValueLocalizations = ValueLocalizations,
            LookupHash = LookupHash,
            OverrideType = OverrideType
        };
    }

    /// <inheritdoc/>
    public override void FromAuditObject(LookupValueAudit audit)
    {
        base.FromAuditObject(audit);

        Id = audit.LookupValueId;
        Key = audit.Key;
        RangeValue = audit.RangeValue;
        Value = audit.Value;
        ValueLocalizations = audit.ValueLocalizations;
        OverrideType = audit.OverrideType;
    }
}