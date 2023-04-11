using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// Represents a lookup value
/// </summary>
public class LookupValue : ApiObjectBase
{
    /// <summary>
    /// The lookup key, unique within a lookup
    /// </summary>
    [Required]
    public string Key { get; set; }

    /// <summary>
    /// The lookup value as JSON
    /// </summary>
    [Required]
    public string Value { get; set; }

    /// <summary>
    /// The localized lookup values
    /// </summary>
    [Localization(nameof(Value))]
    public Dictionary<string, string> ValueLocalizations { get; set; }

    /// <summary>
    /// The lookup range value, unique within a lookup
    /// </summary>
    public decimal? RangeValue { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Key} {Value} {base.ToString()}";
}