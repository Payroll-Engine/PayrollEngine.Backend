using System;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Query for the payroll controller
/// </summary>
public class PayrollQuery
{
    /// <summary>
    /// The tenant id
    /// </summary>
    [Required]
    public int TenantId { get; set; }

    /// <summary>
    /// The payroll id
    /// </summary>
    [Required]
    public int PayrollId { get; set; }

    /// <summary>
    /// The employee id
    /// </summary>
    public int? EmployeeId { get; set; }

    /// <summary>
    /// The regulation date
    /// </summary>
    public DateTime? RegulationDate { get; set; }

    /// <summary>
    /// The evaluation date
    /// </summary>
    public DateTime? EvaluationDate { get; set; }
}