using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Result of a lookup range bracket computation
/// </summary>
// ReSharper disable PropertyCanBeMadeInitOnly.Global
public class LookupRangeResult : IEquatable<LookupRangeResult>
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

    /// <summary>
    /// Default constructor
    /// </summary>
    public LookupRangeResult()
    {
    }

    /// <summary>
    /// Copy constructor
    /// </summary>
    /// <param name="source">The source to copy</param>
    public LookupRangeResult(LookupRangeResult source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        LookupName = source.LookupName;
        RangeMode = source.RangeMode;
        RangeSize = source.RangeSize;
        MatchingBracket = source.MatchingBracket;
        MatchingBrackets = source.MatchingBrackets;
        AllBrackets = source.AllBrackets;
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(LookupRangeResult compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override string ToString() =>
        LookupName;
}
