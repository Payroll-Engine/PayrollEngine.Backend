using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Custom collector result
/// </summary>
// ReSharper disable PropertyCanBeMadeInitOnly.Global
public class CollectorCustomResult : DomainObjectBase, ITagObject,
    IDomainAttributeObject, IEquatable<CollectorCustomResult>
{
    /// <summary>
    /// The wage type result id (immutable)
    /// </summary>
    public int CollectorResultId { get; set; }

    /// <summary>
    /// The collector name (immutable)
    /// </summary>
    public string CollectorName { get; set; }

    /// <summary>
    /// The collector name hash (immutable)
    /// </summary>
    public int CollectorNameHash
    {
        get => CollectorName.ToPayrollHash();
        // ReSharper disable once ValueParameterNotUsed
        set
        {
            // hash is calculated
        }
    }

    /// <summary>
    /// The localized collector names (immutable)
    /// </summary>
    public Dictionary<string, string> CollectorNameLocalizations { get; set; }

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
                    $"Value type of collector custom result must be a number: {value}");
            }

            field = value;
        }
    } = ValueType.Decimal;

    /// <summary>
    /// The collector custom result value (immutable)
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// The collector custom result culture name based on RFC 4646
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
    public CollectorCustomResult()
    {
    }

    /// <inheritdoc/>
    public CollectorCustomResult(CollectorCustomResult copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(CollectorCustomResult compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Source}={Value} [{Start}-{End}] {base.ToString()}";
}