using System;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// The lookup settings
/// </summary>
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
public class LookupSettings : IEquatable<LookupSettings>
{
    /// <summary>
    /// The lookup name
    /// </summary>
    public string LookupName { get; set; }

    /// <summary>
    /// The lookup value field name
    /// </summary>
    public string ValueFieldName { get; set; }

    /// <summary>
    /// The lookup text/display field name
    /// </summary>
    public string TextFieldName { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public LookupSettings()
    {
    }

    /// <summary>
    /// Copy constructor
    /// </summary>
    public LookupSettings(LookupSettings source)
    {
        LookupName = source.LookupName;
        ValueFieldName = source.ValueFieldName;
        TextFieldName = source.TextFieldName;
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(LookupSettings compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc />
    public override string ToString() => LookupName;
}