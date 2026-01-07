using System;
using System.Collections.Generic;
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A wage type result
/// </summary>
public class WageTypeResult : DomainObjectBase, ITagObject, IDomainAttributeObject, IEquatable<WageTypeResult>
{
    /// <summary>
    /// The payroll result id (immutable)
    /// </summary>
    public int PayrollResultId { get; set; }

    /// <summary>
    /// The wage type id (immutable)
    /// </summary>
    public int WageTypeId { get; set; }

    /// <summary>
    /// The wage type number (immutable)
    /// </summary>
    public decimal WageTypeNumber { get; set; }

    /// <summary>
    /// The wage type name (immutable)
    /// </summary>
    public string WageTypeName { get; set; }

    /// <summary>
    /// The localized wage type names (immutable)
    /// </summary>
    public Dictionary<string, string> WageTypeNameLocalizations { get; set; }

    /// <summary>
    /// The value type (immutable)
    /// </summary>
    public ValueType ValueType
    {
        get;
        set
        {
            if (!value.IsNumber())
            {
                throw new ArgumentOutOfRangeException(nameof(value),
                    $"Value type of wage type result must be a number: {value}");
            }

            field = value;
        }
    } = ValueType.Decimal;

    /// <summary>
    /// The wage type result value (immutable)
    /// </summary>
    public decimal Value { get; set; }
    
    /// <summary>
    /// The case field culture name based on RFC 4646
    /// </summary>
    public string Culture { get; set; }

    /// <summary>
    /// The starting date for the value
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
    /// The ending date for the value
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
    public WageTypeResult()
    {
    }

    /// <inheritdoc/>
    protected WageTypeResult(WageTypeResult copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(WageTypeResult compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override string ToString() =>
        $"{WageTypeNumber:##.####} {Value:##.####} {base.ToString()}";
}