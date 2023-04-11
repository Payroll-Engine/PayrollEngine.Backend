using System;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Payroll case change query parameters
/// </summary>
public class PayrollCaseChangeQuery : CaseChangeQuery
{
    /// <summary>
    /// The user id
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// The case type
    /// </summary>
    [Required]
    public CaseType CaseType { get; set; }

    /// <summary>
    /// The employeeId id, mandatory for employee case
    /// </summary>
    public int? EmployeeId { get; set; }

    /// <summary>
    /// The cluster set name
    /// </summary>
    public string ClusterSetName { get; set; }

    /// <summary>
    /// The regulation date
    /// </summary>
    public DateTime? RegulationDate { get; set; }

    /// <summary>
    /// The evaluation date
    /// </summary>
    public DateTime? EvaluationDate { get; set; }
}