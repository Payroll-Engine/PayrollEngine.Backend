using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A wage type custom result
/// </summary>
// ReSharper disable PropertyCanBeMadeInitOnly.Global
public class WageTypeCustomResult : DomainObjectBase, ITagObject, 
    IDomainAttributeObject, IEquatable<WageTypeCustomResult>
{
    /// <summary>
    /// The wage type result id (immutable)
    /// </summary>
    public int WageTypeResultId { get; set; }
        
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
    /// The value source (immutable)
    /// </summary>
    public string Source { get; set; }

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
                    $"Value type of wage type custom result must be a number: {value}");
            }

            field = value;
        }
    } = ValueType.Decimal;

    /// <summary>
    /// The wage type custom result value (immutable)
    /// </summary>
    public decimal Value { get; set; }
    
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
    public WageTypeCustomResult()
    {
    }

    /// <inheritdoc/>
    public WageTypeCustomResult(WageTypeCustomResult copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(WageTypeCustomResult compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Source}={Value} [{Start}-{End}] {base.ToString()}";
}