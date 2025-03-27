using System;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The payroll result info API object
/// </summary>
// ReSharper disable UnusedAutoPropertyAccessor.Global
public class PayrollResult : ApiObjectBase
{
    /// <summary>
    /// The payroll id (immutable)
    /// </summary>
    [Required]
    public int PayrollId { get; set; }

    /// <summary>
    /// The payrun id (immutable)
    /// </summary>
    public int PayrunId { get; set; }

    /// <summary>
    /// The payrun job id (immutable)
    /// </summary>
    [Required]
    public int PayrunJobId { get; set; }

    /// <summary>
    /// The employee id (immutable)
    /// </summary>
    [Required]
    public int EmployeeId { get; set; }

    /// <summary>
    /// The division id (immutable)
    /// </summary>
    [Required]
    public int DivisionId { get; set; }

    /// <summary>
    /// The cycle name (immutable)
    /// </summary>
    [Required]
    public string CycleName { get; set; }

    /// <summary>
    /// The cycle start date (immutable)
    /// </summary>
    [Required]
    public DateTime CycleStart { get; set; }

    /// <summary>
    /// The cycle end date (immutable)
    /// </summary>
    [Required]
    public DateTime CycleEnd { get; set; }

    /// <summary>
    /// The period name (immutable)
    /// </summary>
    [Required]
    public string PeriodName { get; set; }

    /// <summary>
    /// The period start date (immutable)
    /// </summary>
    [Required]
    public DateTime PeriodStart { get; set; }

    /// <summary>
    /// The period end date (immutable)
    /// </summary>
    public DateTime PeriodEnd { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"result: {PeriodName} for employee {EmployeeId} on division {DivisionId} {base.ToString()}";
}