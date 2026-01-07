using System;
using System.Collections.Generic;
// ReSharper disable MemberCanBeProtected.Global

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A value lookup
/// </summary>
public class Lookup : TrackDomainObject<LookupAudit>, IDomainAttributeObject,
    IDerivableObject, INamedObject, INamespaceObject, IEquatable<Lookup>
{
    /// <summary>
    /// The lookup name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The localized lookup names
    /// </summary>
    public Dictionary<string, string> NameLocalizations { get; set; }

    /// <summary>
    /// The lookup description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The localized lookup descriptions
    /// </summary>
    public Dictionary<string, string> DescriptionLocalizations { get; set; }

    /// <summary>
    /// The override type
    /// </summary>
    public OverrideType OverrideType { get; set; }

    /// <summary>
    /// Lookup range mode
    /// </summary>
    public LookupRangeMode RangeMode { get; set; }

    /// <summary>
    /// The lookup range size
    /// </summary>
    public decimal? RangeSize { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <inheritdoc/>
    public Lookup()
    {
    }

    /// <inheritdoc/>
    protected Lookup(Lookup copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }
    
    /// <inheritdoc/>
    public void ApplyNamespace(string @namespace)
    {
        Name = Name.EnsureNamespace(@namespace);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(Lookup compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override LookupAudit ToAuditObject()
    {
        if (!this.HasId())
        {
            return null;
        }
        return new()
        {
            LookupId = Id,
            Name = Name,
            NameLocalizations = NameLocalizations,
            Description = Description,
            DescriptionLocalizations = DescriptionLocalizations,
            RangeMode = RangeMode,
            RangeSize = RangeSize,
            Attributes = Attributes
        };
    }

    /// <inheritdoc/>
    public override void FromAuditObject(LookupAudit audit)
    {
        base.FromAuditObject(audit);

        Id = audit.LookupId;
        Name = audit.Name;
        NameLocalizations = audit.NameLocalizations;
        Description = audit.Description;
        DescriptionLocalizations = audit.DescriptionLocalizations;
        RangeMode = audit.RangeMode;
        RangeSize = audit.RangeSize;
        Attributes = audit.Attributes;
    }
}