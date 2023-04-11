using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A collector result
/// </summary>
public class CollectorResult : DomainObjectBase, ITagObject, IDomainAttributeObject, IEquatable<CollectorResult>
{
    /// <summary>
    /// The payroll result id (immutable)
    /// </summary>
    public int PayrollResultId { get; set; }

    /// <summary>
    /// The collector id (immutable)
    /// </summary>
    public int CollectorId { get; set; }

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
    /// The collection type (immutable)
    /// </summary>
    public CollectType CollectType { get; set; }

    private ValueType valueType = ValueType.Decimal;
    /// <summary>
    /// The value type (immutable)
    /// </summary>
    public ValueType ValueType
    {
        get => valueType;
        set
        {
            if (!value.IsNumber())
            {
                throw new ArgumentOutOfRangeException(nameof(value),
                    $"Value type of collector result must be a number: {value}");
            }
            valueType = value;
        }
    }

    /// <summary>
    /// The collector result value (immutable)
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// The starting date for the value (immutable)
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
    /// The ending date for the value (immutable)
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
    public CollectorResult()
    {
    }

    /// <inheritdoc/>
    public CollectorResult(CollectorResult copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(CollectorResult compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override string ToString() =>
        $"{CollectorName}={Value:##.####} {base.ToString()}";
}