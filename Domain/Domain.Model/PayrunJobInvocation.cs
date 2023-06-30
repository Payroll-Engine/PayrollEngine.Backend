using System;
using System.Collections.Generic;
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A payrun job invocation
/// </summary>
public class PayrunJobInvocation : IEquatable<PayrunJobInvocation>
{
    /// <summary>
    /// The job name (immutable)
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The job owner (immutable)
    /// </summary>
    public string Owner { get; set; }

    /// <summary>
    /// The payrun id (immutable)
    /// </summary>
    public int PayrunId { get; set; }

    /// <summary>
    /// The user id (immutable)
    /// </summary>
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
    /// The retro payrun jobs (immutable)
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
    public DateTime PeriodStart { get; set; }

    /// <summary>
    /// The evaluation date (immutable)
    /// </summary>
    public DateTime? EvaluationDate { get; set; }

    /// <summary>
    /// The execution reason (immutable)
    /// </summary>
    public string Reason { get; set; }

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


    /// <summary>Initializes a new instance</summary>
    public PayrunJobInvocation()
    {
    }

    /// <summary>Initializes a new instance from a copy</summary>
    /// <param name="copySource">The copy source</param>
    public PayrunJobInvocation(PayrunJobInvocation copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(PayrunJobInvocation compare) =>
        CompareTool.EqualProperties(this, compare);
}