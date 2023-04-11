using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A payrun job
/// </summary>
public class PayrunJob : DomainObjectBase, INamedObject, IDomainAttributeObject, IEquatable<PayrunJob>
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
    /// The payroll id (immutable)
    /// </summary>
    public int PayrollId { get; set; }

    /// <summary>
    /// The division id (immutable)
    /// </summary>
    public int DivisionId { get; set; }

    /// <summary>
    /// The user id (immutable)
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// The parent payrun job id, e.g. the parent retro pay run job (immutable)
    /// </summary>
    public int? ParentJobId { get; set; }

    /// <summary>
    /// Gets a value indicating whether this instance is retro payrun job
    /// </summary>
    [JsonIgnore]
    public bool IsRetroJob => ParentJobId.HasValue;

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
    /// The payrun job status
    /// </summary>
    public PayrunJobStatus JobStatus { get; set; }

    /// <summary>
    /// The payrun job result
    /// </summary>
    public PayrunJobResult JobResult { get; set; }

    /// <summary>
    /// The culture including the calendar (immutable)
    /// </summary>
    public string Culture { get; set; }

    /// <summary>
    /// The cycle name (immutable)
    /// </summary>
    public string CycleName { get; set; }

    /// <summary>
    /// The cycle start date (immutable)
    /// </summary>
    public DateTime CycleStart { get; set; }

    /// <summary>
    /// The cycle end date (immutable)
    /// </summary>
    public DateTime CycleEnd { get; set; }

    /// <summary>
    /// The period name (immutable)
    /// </summary>
    public string PeriodName { get; set; }

    /// <summary>
    /// The period start date (immutable)
    /// </summary>
    public DateTime PeriodStart { get; set; }

    /// <summary>
    /// The period end date (immutable)
    /// </summary>
    public DateTime PeriodEnd { get; set; }

    /// <summary>
    /// The evaluation date (immutable)
    /// </summary>
    public DateTime EvaluationDate { get; set; }

    /// <summary>
    /// The execution reason (immutable)
    /// </summary>
    public string Reason { get; set; }

    /// <summary>
    /// Total employee count
    /// </summary>
    public int TotalEmployeeCount { get; set; }

    /// <summary>
    /// Processed employee count
    /// </summary>
    public int ProcessedEmployeeCount { get; set; }

    /// <summary>
    /// The job start date
    /// </summary>
    public DateTime JobStart { get; set; }

    /// <summary>
    /// The job end date
    /// </summary>
    public DateTime? JobEnd { get; set; }

    /// <summary>
    /// The job message
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// The job error message
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// The payrun employees
    /// </summary>
    public List<PayrunJobEmployee> Employees { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <inheritdoc/>
    public PayrunJob()
    {
    }

    /// <inheritdoc/>
    public PayrunJob(PayrunJob copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>
    /// Get job time period
    /// </summary>
    /// <returns>Date period from the job start until the job end</returns>
    public DatePeriod GetEvaluationPeriod() =>
        new(PeriodStart, PeriodEnd);

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(PayrunJob compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Name} ({JobStatus}) {base.ToString()}";
}