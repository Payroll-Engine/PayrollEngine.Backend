using System.Collections.Generic;

namespace PayrollEngine.Api.Model;

/// <summary>
/// Result of a lookup range bracket computation
/// </summary>
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
public class LookupRangeResult
{
    /// <summary>
    /// The lookup name
    /// </summary>
    public string LookupName { get; set; }

    /// <summary>
    /// The lookup range mode
    /// </summary>
    public LookupRangeMode RangeMode { get; set; }

    /// <summary>
    /// The lookup range size
    /// </summary>
    public decimal? RangeSize { get; set; }

    /// <summary>
    /// Range brackets
    /// </summary>
    public List<LookupRangeBracket> Brackets { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        LookupName;
}
