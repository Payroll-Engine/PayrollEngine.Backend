using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A payroll payrun parameter
/// </summary>
public class PayrunParameter : DomainObjectBase, IDomainAttributeObject, IEquatable<PayrunParameter>
{
    /// <summary>
    /// The payrun parameter name (immutable)
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The localized wage type names
    /// </summary>
    public Dictionary<string, string> NameLocalizations { get; set; }

    /// <summary>
    /// The payrun parameter description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The localized payrun parameter descriptions
    /// </summary>
    public Dictionary<string, string> DescriptionLocalizations { get; set; }

    /// <summary>
    /// The parameter mandatory state
    /// </summary>
    public bool Mandatory { get; set; }

    /// <summary>
    /// The parameter value (JSON)
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// The parameter value type
    /// </summary>
    public ValueType ValueType { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PayrunParameter"/> class
    /// </summary>
    public PayrunParameter()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PayrunParameter"/> class
    /// </summary>
    /// <param name="copySource">The copy source.</param>
    public PayrunParameter(PayrunParameter copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(PayrunParameter compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Name} {base.ToString()}";
}