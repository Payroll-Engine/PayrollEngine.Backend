using System;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A lookup range bracket with computed bounds
/// </summary>
// ReSharper disable PropertyCanBeMadeInitOnly.Global
public class LookupRangeBracket : IEquatable<LookupRangeBracket>
{
    /// <summary>
    /// The lookup value key
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// The lookup value as JSON
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// The range start (lower bound)
    /// </summary>
    public decimal LowerBound { get; set; }

    /// <summary>
    /// The range end (upper bound), null for unbounded last bracket
    /// </summary>
    public decimal? UpperBound { get; set; }

    /// <summary>
    /// The original range value from the lookup value
    /// </summary>
    public decimal? RangeValue { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public LookupRangeBracket()
    {
    }

    /// <summary>
    /// Copy constructor
    /// </summary>
    /// <param name="source">The source to copy</param>
    public LookupRangeBracket(LookupRangeBracket source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        Key = source.Key;
        Value = source.Value;
        LowerBound = source.LowerBound;
        UpperBound = source.UpperBound;
        RangeValue = source.RangeValue;
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(LookupRangeBracket compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Key}: {LowerBound} - {UpperBound}";
}
