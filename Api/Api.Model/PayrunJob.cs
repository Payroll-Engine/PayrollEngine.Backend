using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The payrun job API object
/// </summary>
public class PayrunJob : ApiObjectBase
{
    /// <summary>
    /// The payrun job name (immutable)
    /// </summary>
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// The job owner (immutable)
    /// </summary>
    public string Owner { get; set; }

    /// <summary>
    /// The payrun id (immutable)
    /// </summary>
    [Required]
    public int PayrunId { get; set; }

    /// <summary>
    /// The payroll id (immutable)
    /// </summary>
    [Required]
    public int PayrollId { get; set; }

    /// <summary>
    /// The division id (immutable)
    /// </summary>
    public int DivisionId { get; set; }

    /// <summary>
    /// The user id (immutable)
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// The parent payrun job id, e.g. the parent retro pay run job (immutable)
    /// </summary>
    public int? ParentJobId { get; set; }

    /// <summary>
    /// The payrun retro pay mode (immutable)
    /// </summary>
    public RetroPayMode RetroPayMode { get; set; }

    /// <summary>
    /// The payrun job status (immutable)
    /// </summary>
    public PayrunJobStatus JobStatus { get; set; }

    /// <summary>
    /// The payrun job result
    /// </summary>
    public PayrunJobResult JobResult { get; set; }

    /// <summary>
    /// The job tags (immutable)
    /// </summary>
    public List<string> Tags { get; set; }

    /// <summary>
    /// The forecast name (immutable)
    /// </summary>
    public string Forecast { get; set; }

    /// <summary>
    /// The culture including the calendar (immutable)
    /// </summary>
    public string Culture { get; set; }

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
    [Required]
    public DateTime PeriodEnd { get; set; }

    /// <summary>
    /// The evaluation date (immutable)
    /// </summary>
    [Required]
    public DateTime EvaluationDate { get; set; }

    /// <summary>
    /// The execution reason (immutable)
    /// </summary>
    [Required]
    public string Reason { get; set; }

    /// <summary>
    /// Total employee count (immutable)
    /// </summary>
    public int TotalEmployeeCount { get; set; }

    /// <summary>
    /// Processed employee count (immutable)
    /// </summary>
    public int ProcessedEmployeeCount { get; set; }

    /// <summary>
    /// The job start date (immutable)
    /// </summary>
    [Required]
    public DateTime JobStart { get; set; }

    /// <summary>
    /// The job end date (immutable)
    /// </summary>
    public DateTime? JobEnd { get; set; }

    /// <summary>
    /// The job message (immutable)
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// The job error message (immutable)
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// The payrun employees
    /// </summary>
    public PayrunJobEmployee[] Employees { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }
}