using System;
using System.Globalization;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A case value used in national, company and employee case
/// </summary>
// ReSharper disable PropertyCanBeMadeInitOnly.Global
public class CaseValue : DomainObjectBase, IDomainAttributeObject, IEquatable<CaseValue>
{
    private static CultureInfo DefaultCulture => CultureInfo.DefaultThreadCurrentCulture ?? CultureInfo.InvariantCulture;

    /// <summary>
    /// The division id (immutable)
    /// Mandatory for case values with local value scope <see cref="CaseField.ValueScope"/>
    /// </summary>
    public int? DivisionId { get; set; }

    /// <summary>
    /// The employee id, mandatory for employee case changes (immutable)
    /// </summary>
    public int? EmployeeId { get; set; }

    /// <summary>
    /// The associated case name (immutable)
    /// </summary>
    public string CaseName { get; set; }

    /// <summary>
    /// The localized case names
    /// </summary>
    public Dictionary<string, string> CaseNameLocalizations { get; set; }

    /// <summary>
    /// The associated case field name (immutable)
    /// </summary>
    public string CaseFieldName { get; set; }

    /// <summary>
    /// The localized case field names
    /// </summary>
    public Dictionary<string, string> CaseFieldNameLocalizations { get; set; }

    /// <summary>
    /// The case slot (immutable)
    /// </summary>
    public string CaseSlot { get; set; }

    /// <summary>
    /// The localized case slots
    /// </summary>
    public Dictionary<string, string> CaseSlotLocalizations { get; set; }

    /// <summary>
    /// The type of the value (immutable)
    /// </summary>
    public ValueType ValueType
    {
        get;
        set
        {
            field = value;
            UpdateNumericValue();
        }
    } = ValueType.String;

    /// <summary>
    /// The case value (JSON format)
    /// </summary>
    public string Value
    {
        get;
        set
        {
            field = value;
            UpdateNumericValue();
        }
    }

    private void UpdateNumericValue() =>
        NumericValue = ValueConvert.ToNumber(Value, ValueType, GetCultureInfo());

    /// <summary>
    /// The case numeric value
    /// </summary>
    public decimal? NumericValue { get; set; }

    /// <summary>
    /// The case field culture name based on RFC 4646
    /// </summary>
    public string Culture { get; set; }

    /// <summary>
    /// The case relation
    /// </summary>
    public CaseRelationReference CaseRelation { get; set; }

    /// <summary>
    /// Cancellation date
    /// </summary>
    public DateTime? CancellationDate { get; set; }

    /// <summary>
    /// The starting date for the value
    /// </summary>
    public DateTime? Start { get; set; }

    /// <summary>
    /// The ending date for the value
    /// </summary>
    public DateTime? End { get; set; }

    /// <summary>
    /// The forecast name
    /// </summary>
    public string Forecast { get; set; }

    /// <summary>
    /// The case value tags
    /// </summary>
    public List<string> Tags { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <inheritdoc/>
    public CaseValue()
    {
    }

    /// <inheritdoc/>
    protected CaseValue(CaseValue copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(CaseValue compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <summary>
    /// Get native value
    /// </summary>
    /// <returns>The .net value</returns>
    public object GetValue() =>
        ValueConvert.ToValue(Value, ValueType, GetCultureInfo());

    /// <summary>
    /// Set native value
    /// </summary>
    public void SetValue(object value) =>
        Value = ValueConvert.ToJson(value);

    /// <summary>
    /// Check if a time is withing the case value period
    /// </summary>
    /// <param name="moment">The moment to test</param>
    /// <returns>True, if the case value is relevant for the requested moment</returns>
    public bool IsWithing(DateTime moment) =>
        new DatePeriod(Start, End).IsWithin(moment);

    /// <inheritdoc/>
    public override string ToString()
    {
        var toString = string.IsNullOrWhiteSpace(CaseSlot) ?
            $"{Value} ({CaseName}.{CaseFieldName}) {base.ToString()}" :
            $"{Value} ({CaseName}.{CaseFieldName}[{CaseSlot}]) {base.ToString()}";

        // without limits
        if (!Start.HasValue && !End.HasValue)
        {
            return toString;
        }

        // with limits
        return $"{toString} [{new DatePeriod(Start, End)}]";
    }

    private CultureInfo GetCultureInfo()
    {
        if (string.IsNullOrWhiteSpace(Culture))
        {
            return DefaultCulture;
        }
        return CultureInfo.GetCultureInfo(Culture);
    }
}