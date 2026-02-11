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
    /// The range start value
    /// </summary>
    public decimal RangeStart { get; set; }

    /// <summary>
    /// The range end value (unbound bracket: Decimal.MaxValue)
    /// </summary>
    public decimal RangeEnd { get; set; }

    /// <summary>
    /// Test for unlimited bracket
    /// </summary>
    public bool IsUnlimited => RangeEnd == Decimal.MaxValue;

    /// <summary>
    /// Set unlimited bracket
    /// </summary>
    public void SetUnlimited() => RangeEnd = Decimal.MaxValue;

    /// <summary>
    /// The bracket range value
    /// </summary>
    /// <remarks>
    /// For threshold lookups, the value within the matching bracket is displayed.
    /// For progressive lookups, it is the sum of all the matching brackets, excluding the final one, which has its own value.
    /// For all other lookup types, the value is null.
    /// </remarks>
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
        RangeStart = source.RangeStart;
        RangeEnd = source.RangeEnd;
        RangeValue = source.RangeValue;
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(LookupRangeBracket compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Key}: {RangeStart} - {RangeEnd}";
}
