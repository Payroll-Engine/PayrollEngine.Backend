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
    /// The matching bracket for threshold mode, null for progressive
    /// </summary>
    public LookupRangeBracket MatchingBracket { get; set; }

    /// <summary>
    /// All matching brackets for progressive mode, null for threshold
    /// </summary>
    public List<LookupRangeBracket> MatchingBrackets { get; set; }

    /// <summary>
    /// All range brackets
    /// </summary>
    public List<LookupRangeBracket> AllBrackets { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        LookupName;
}
