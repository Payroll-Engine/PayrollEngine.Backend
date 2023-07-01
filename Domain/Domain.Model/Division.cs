using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A division
/// </summary>
public class Division : DomainObjectBase, INamedObject, IDomainAttributeObject, IEquatable<Division>
{
    /// <summary>
    /// The division name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The localized division names
    /// </summary>
    public Dictionary<string, string> NameLocalizations { get; set; }

    /// <summary>
    /// The division culture name based on RFC 4646 (fallback: tenant culture)
    /// </summary>
    public string Culture { get; set; }

    /// <summary>
    /// The division calendar (fallback: tenant calendar)
    /// </summary>
    public string Calendar { get; set; }
    
    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <inheritdoc/>
    public Division()
    {
    }

    /// <inheritdoc/>
    public Division(Division copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(Division compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Name} {base.ToString()}";
}