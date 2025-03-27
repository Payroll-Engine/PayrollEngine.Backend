using System;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Lookup value data in a specific language
/// </summary>
// ReSharper disable PropertyCanBeMadeInitOnly.Global
public class LookupValueData : IEquatable<LookupValueData>
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

    /// <summary>
    /// Default constructor
    /// </summary>
    public LookupValueData()
    {
    }

    /// <summary>
    /// Copy constructor
    /// </summary>
    /// <param name="source">The source to copy</param>
    public LookupValueData(LookupValueData source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        Key = source.Key;
        RangeValue = source.RangeValue;
        Value = source.Value;
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(LookupValueData compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Key} ({Value})";
}