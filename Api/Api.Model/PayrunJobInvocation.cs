// ReSharper disable UnusedAutoPropertyAccessor.Global
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The payrun job invocation API object
/// </summary>
public class PayrunJobInvocation : ApiObjectBase
{
    /// <summary>
    /// The payrun id (immutable)
    /// </summary>
    [Required]
    public int PayrunId { get; set; }

    /// <summary>
    /// The user id (immutable)
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// The payrun job id (immutable)
    /// </summary>
    public int PayrunJobId { get; set; }

    /// <summary>
    /// The parent payrun job id, e.g. the parent retro pay run job (immutable)
    /// </summary>
    public int? ParentJobId { get; set; }

    /// <summary>
    /// The job name (immutable)
    /// </summary>
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// The job owner (immutable)
    /// </summary>
    public string Owner { get; set; }

    /// <summary>
    /// The retro payrun jobs, requires the ParentJobId (immutable)
    /// </summary>
    public List<RetroPayrunJob> RetroJobs { get; set; }

    /// <summary>
    /// The job tags (immutable)
    /// </summary>
    public List<string> Tags { get; set; }

    /// <summary>
    /// The forecast name (immutable)
    /// </summary>
    public string Forecast { get; set; }

    /// <summary>
    /// The payrun retro pay mode (immutable)
    /// </summary>
    public RetroPayMode RetroPayMode { get; set; }

    /// <summary>
    /// The payrun job result
    /// </summary>
    public PayrunJobResult JobResult { get; set; }

    /// <summary>
    /// The payrun job status
    /// </summary>
    public PayrunJobStatus JobStatus { get; set; }

    /// <summary>
    /// The period start date (immutable)
    /// </summary>
    [Required]
    public DateTime PeriodStart { get; set; }

    /// <summary>
    /// The evaluation date (immutable)
    /// </summary>
    public DateTime? EvaluationDate { get; set; }

    /// <summary>
    /// The execution reason (immutable)
    /// </summary>
    [Required]
    public string Reason { get; set; }

    /// <summary>
    /// Store empty employee results (default: false)
    /// </summary>
    public bool StoreEmptyResults { get; set; }

    /// <summary>
    /// The function log level, default is information
    /// </summary>
    public LogLevel LogLevel { get; set; } = LogLevel.Information;

    /// <summary>
    /// The payrun employee identifiers
    /// </summary>
    public List<string> EmployeeIdentifiers { get; set; }

    /// <summary>
    /// Payrun job attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }
}