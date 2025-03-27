using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// The case field including the slot and values
/// </summary>
// ReSharper disable UnusedAutoPropertyAccessor.Global
public class CaseFieldSet : CaseField, IEquatable<CaseFieldSet>
{
    /// <summary>
    /// The case field display name
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// The case slot
    /// </summary>
    public string CaseSlot { get; set; }

    /// <summary>
    /// The localized case slots
    /// </summary>
    public Dictionary<string, string> CaseSlotLocalizations { get; set; }

    /// <summary>
    /// The case value (JSON format)
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// The starting date for the value
    /// </summary>
    public DateTime? Start { get; set; }

    /// <summary>
    /// The ending date for the value
    /// </summary>
    public DateTime? End { get; set; }

    /// <summary>
    /// Cancellation date
    /// </summary>
    public DateTime? CancellationDate { get; set; }

    /// <summary>
    /// Test for existing value
    /// </summary>
    [JsonIgnore]
    public bool HasValue => !string.IsNullOrWhiteSpace(Value);

    /// <inheritdoc/>
    public CaseFieldSet()
    {
    }

    /// <inheritdoc/>
    public CaseFieldSet(CaseFieldSet copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <inheritdoc/>
    public CaseFieldSet(CaseField caseField) :
        base(caseField)
    {
    }

    /// <inheritdoc/>
    public CaseFieldSet(CaseValue caseValue)
    {
        DisplayName = caseValue.CaseName;
        CaseSlot = caseValue.CaseSlot;
        CaseSlotLocalizations = caseValue.CaseSlotLocalizations.Copy();
        ValueType = caseValue.ValueType;
        Name = caseValue.CaseFieldName;
        Value = caseValue.Value;
        Start = caseValue.Start;
        End = caseValue.End;
        CancellationDate = caseValue.CancellationDate;
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(CaseFieldSet compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <summary>Get native value</summary>
    /// <param name="culture">The culture</param>
    /// <returns>The .net value</returns>
    public object GetValue(CultureInfo culture) =>
        ValueConvert.ToValue(Value, ValueType, culture);

    /// <summary>
    /// Set native value
    /// </summary>
    public void SetValue(object value) =>
        Value = ValueConvert.ToJson(value);

    /// <inheritdoc/>
    public override string ToString() =>
        string.IsNullOrWhiteSpace(CaseSlot) ? $" {Name}={Value} {ToObjectString()}" :
            $" {Name}:{CaseSlot}={Value} {ToObjectString()}";
}