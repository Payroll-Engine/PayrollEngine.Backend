using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace PayrollEngine.Api.Model;

/// <summary>
/// Case change with multiple case values of one case type
/// </summary>
public class CaseChange : ApiObjectBase
{
    /// <summary>
    /// The change user id
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// The employee id, mandatory for employee case changes (immutable)
    /// </summary>
    public int? EmployeeId { get; set; }

    /// <summary>
    /// The division id (immutable)
    /// If present, this values overrides all case value divisions  <see cref="CaseValue.DivisionId"/>
    /// </summary>
    public int? DivisionId { get; set; }

    /// <summary>
    /// The cancellation type
    /// </summary>
    public CaseCancellationType CancellationType { get; set; }

    /// <summary>
    /// The cancellation case id (immutable)
    /// </summary>
    public int? CancellationId { get; set; }

    /// <summary>
    /// The cancellation date (immutable)
    /// </summary>
    public DateTime? CancellationDate { get; set; }

    /// <summary>
    /// The change reason
    /// </summary>
    public string Reason { get; set; }

    /// <summary>
    /// The validation case name, triggers the case validation (optional)
    /// </summary>
    public string ValidationCaseName { get; set; }

    /// <summary>
    /// The forecast name
    /// </summary>
    public string Forecast { get; set; }

    /// <summary>
    /// The case values
    /// </summary>
    // ReSharper disable once CollectionNeverUpdated.Global
    public List<CaseValue> Values { get; set; }

    /// <summary>
    /// The ignored case values
    /// </summary>
    public List<CaseValue> IgnoredValues { get; set; }
}