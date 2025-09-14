using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The payroll case value API object (immutable)
/// </summary>
// ReSharper disable UnusedAutoPropertyAccessor.Global
public class CaseValue : ApiObjectBase
{
    /// <summary>
    /// The division id (immutable)
    /// Mandatory for case values with local value scope <see cref="CaseField.ValueScope"/>
    /// </summary>
    public int? DivisionId { get; set; }

    /// <summary>
    /// The employee id, mandatory for employee case changes
    /// </summary>
    public int? EmployeeId { get; set; }

    /// <summary>
    /// The associated case name
    /// </summary>
    [StringLength(128)]
    public string CaseName { get; set; }

    /// <summary>
    /// The localized case names
    /// </summary>
    public Dictionary<string, string> CaseNameLocalizations { get; set; }

    /// <summary>
    /// The associated case field name
    /// </summary>
    [Required]
    [StringLength(128)]
    public string CaseFieldName { get; set; }

    /// <summary>
    /// The localized case field names
    /// </summary>
    public Dictionary<string, string> CaseFieldNameLocalizations { get; set; }

    /// <summary>
    /// The case slot
    /// </summary>
    [StringLength(128)]
    public string CaseSlot { get; set; }

    /// <summary>
    /// The localized case slots
    /// </summary>
    public Dictionary<string, string> CaseSlotLocalizations { get; set; }

    /// <summary>
    /// The type of the value
    /// </summary>
    public ValueType ValueType { get; set; }

    /// <summary>
    /// The case value (JSON format)
    /// </summary>
    public string Value { get; set; }

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
    [StringLength(128)]
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
    public override string ToString() => string.IsNullOrWhiteSpace(CaseSlot) ?
        $"{Value} ({CaseName}.{CaseFieldName}) {base.ToString()}" :
        $"{Value} ({CaseName}.{CaseFieldName}[{CaseSlot}]) {base.ToString()}";
}