namespace PayrollEngine.Api.Model;

/// <summary>
/// A lookup range bracket with computed bounds
/// </summary>
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
public class LookupRangeBracket
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
    /// The brackets range value
    /// </summary>
    /// <remarks>
    /// Threshold lookup: value within the matching bracket.
    /// Progressive lookup: matching bracket value range, except the last one which the value within his range.
    /// Other: null.
    /// </remarks>
    public decimal? RangeValue { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Key}: {RangeStart} - {RangeEnd}";
}
