using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The lookup value audit API object (immutable)
/// </summary>
public class LookupValueAudit : ApiObjectBase
{
    /// <summary>
    /// The lookup value id
    /// </summary>
    [Required]
    public int LookupValueId { get; set; }

    /// <summary>
    /// The lookup key
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
    /// The lookup range value
    /// </summary>
    public decimal? RangeValue { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Key} {Value} {base.ToString()}";
}