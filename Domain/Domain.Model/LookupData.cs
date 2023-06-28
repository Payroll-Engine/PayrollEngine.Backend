using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A value lookup in a specific language
/// </summary>
public class LookupData : IEquatable<LookupData>
{
    /// <summary>
    /// The lookup name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The values culture
    /// </summary>
    public string Culture { get; set; }

    /// <summary>
    /// The lookup values
    /// </summary>
    public List<LookupValueData> Values { get; set; }

    /// <summary>
    /// The lookup range size
    /// </summary>
    public decimal? RangeSize { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public LookupData()
    {
    }

    /// <summary>
    /// Copy constructor
    /// </summary>
    /// <param name="source">The source to copy</param>
    public LookupData(LookupData source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        Name = source.Name;
        Culture = source.Culture;
        Values = source.Values.Copy();
        RangeSize = source.RangeSize;
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(LookupData compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override string ToString() =>
        Name;
}