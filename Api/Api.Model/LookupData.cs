using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PayrollEngine.Serialization;

namespace PayrollEngine.Api.Model;

/// <summary>
/// Localized lookup for UI cases like list/grid selections
/// </summary>
public class LookupData
{
    /// <summary>
    /// The lookup name
    /// </summary>
    [Required]
    [StringLength(128)]
    public string Name { get; set; }

    /// <summary>
    /// The language of the values
    /// </summary>
    [JsonConverter(typeof(StringNullableEnumConverter<Language?>))]
    public Language? Language { get; set; }

    /// <summary>
    /// The lookup values
    /// </summary>
    [Required]
    public LookupValueData[] Values { get; set; }
        
    /// <summary>
    /// The lookup range size
    /// </summary>
    public decimal? RangeSize { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Name} {base.ToString()}";
}