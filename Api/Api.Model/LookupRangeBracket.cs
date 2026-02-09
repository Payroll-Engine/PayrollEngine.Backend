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

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Key}: {LowerBound} - {UpperBound}";
}
