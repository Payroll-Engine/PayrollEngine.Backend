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
    /// Range brackets
    /// </summary>
    public List<LookupRangeBracket> Brackets { get; set; }

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
        Brackets = source.Brackets.Copy();
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
