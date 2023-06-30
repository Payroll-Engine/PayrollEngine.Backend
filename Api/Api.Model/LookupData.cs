// ReSharper disable UnusedAutoPropertyAccessor.Global
using System.ComponentModel.DataAnnotations;

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
    /// The values culture
    /// </summary>
    public string Culture { get; set; }

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