using System;
using System.ComponentModel.DataAnnotations;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace PayrollEngine.Api.Model;

/// <summary>
/// Case change setup
/// </summary>
public class CaseChangeSetup
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
    /// The case to cancel, root case name specifies the target case
    /// </summary>
    public int? CancellationId { get; set; }

    /// <summary>
    /// The forecast name
    /// </summary>
    public string Forecast { get; set; }

    /// <summary>
    /// Case change created date
    /// </summary>
    public DateTime? Created { get; set; }

    /// <summary>
    /// The change reason
    /// </summary>
    public string Reason { get; set; }

    /// <summary>
    /// The setup root case
    /// </summary>
    [Required]
    public CaseSetup Case { get; set; }

    /// <summary>
    /// The case validation issues
    /// </summary>
    public CaseValidationIssue[] Issues { get; set; }
}