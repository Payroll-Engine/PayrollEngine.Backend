using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A payroll regulation
/// </summary>
public class Regulation : DomainObjectBase, INamedObject, IDomainAttributeObject, IEquatable<Regulation>
{
    /// <summary>
    /// The regulation name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The localized regulation names
    /// </summary>
    public Dictionary<string, string> NameLocalizations { get; set; }

    /// <summary>
    /// The regulation namespace
    /// </summary>
    public string Namespace { get; set; }

    /// <summary>
    /// The regulation version, unique per regulation name
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Shared regulation (immutable)
    /// </summary>
    public bool SharedRegulation { get; set; }

    /// <summary>
    /// The date the regulation to be in force, anytime if undefined
    /// </summary>
    public DateTime? ValidFrom { get; set; }

    /// <summary>
    /// The owner name
    /// </summary>
    public string Owner { get; set; }

    /// <summary>
    /// The regulation description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The localized regulation descriptions
    /// </summary>
    public Dictionary<string, string> DescriptionLocalizations { get; set; }

    /// <summary>
    /// Required base regulations
    /// </summary>
    public List<string> BaseRegulations { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <inheritdoc/>
    public Regulation()
    {
    }

    /// <inheritdoc/>
    public Regulation(Regulation copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(Regulation compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Name} {base.ToString()}";
}