using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A payrun result
/// </summary>
// ReSharper disable PropertyCanBeMadeInitOnly.Global
public class PayrunResult : DomainObjectBase, ITagObject, IDomainAttributeObject, IEquatable<PayrunResult>
{
    /// <summary>
    /// The payroll result id (immutable)
    /// </summary>
    public int PayrollResultId { get; set; }

    /// <summary>
    /// The result source (immutable)
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// The result name (immutable)
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The localized result names
    /// </summary>
    public Dictionary<string, string> NameLocalizations { get; set; }

    /// <summary>
    /// The result slot (immutable)
    /// </summary>
    public string Slot { get; set; }

    /// <summary>
    /// The value type (immutable)
    /// </summary>
    public ValueType ValueType { get; set; }

    /// <summary>
    /// The case result value (immutable)
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// The numeric result value (immutable)
    /// </summary>
    public decimal? NumericValue { get; set; }
    
    /// <summary>
    /// The case field culture name based on RFC 4646
    /// </summary>
    public string Culture { get; set; }

    /// <summary>
    /// The period starting date for the value
    /// </summary>
    public DateTime Start { get; set; }
    
    /// <summary>
    /// The starting date hash code (immutable)
    /// </summary>
    public int StartHash
    {
        get => Start.GetPastDaysCount();
        // ReSharper disable once ValueParameterNotUsed
        set
        {
            // hash is calculated
        }
    }

    /// <summary>
    /// The period ending date for the value
    /// </summary>
    public DateTime End { get; set; }

    /// <summary>
    /// The result tags
    /// </summary>
    public List<string> Tags { get; set; }

    /// <summary>
    /// The result attributes (immutable)
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <inheritdoc/>
    public PayrunResult()
    {
    }

    /// <inheritdoc/>
    public PayrunResult(PayrunResult copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(PayrunResult compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Name}={Value} [{Start}-{End}] {base.ToString()}";
}