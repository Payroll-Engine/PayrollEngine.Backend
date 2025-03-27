
namespace PayrollEngine.Api.Model;

/// <summary>
/// Lookup value date in a specific language
/// </summary>
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
public class LookupValueData
{
    /// <summary>
    /// The lookup key
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// The lookup value as JSON
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// The lookup range value
    /// </summary>
    public decimal? RangeValue { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Key} {Value}";
}