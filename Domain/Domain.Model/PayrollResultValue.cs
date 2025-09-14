using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Payroll result value
/// </summary>
public class PayrollResultValue : IEquatable<PayrollResultValue>
{
    /// <summary>
    /// The payroll result id
    /// </summary>
    public int PayrollResultId { get; set; }

    /// <summary>
    /// The creation date
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// The result kind <see cref="ResultKind"/>
    /// </summary>
    public ResultKind ResultKind { get; set; }

    /// <summary>
    /// The result id (e.g. the collector id)
    /// </summary>
    public int ResultId { get; set; }

    /// <summary>
    /// The result parent id (e.g. wage typo on custom wage type)
    /// </summary>
    public int ResultParentId { get; set; }

    /// <summary>
    /// The result creation date
    /// </summary>
    public DateTime ResultCreated { get; set; }

    /// <summary>
    /// The result number (e.g. wage type number)
    /// </summary>
    public decimal ResultNumber { get; set; }

    /// <summary>
    /// The kind name, wage type number or collect mode
    /// </summary>
    public string KindName { get; set; }

    /// <summary>
    /// The result start date
    /// </summary>
    public DateTime ResultStart { get; set; }

    /// <summary>
    /// The result end date
    /// </summary>
    public DateTime ResultEnd { get; set; }

    /// <summary>
    /// The result type
    /// </summary>
    public ValueType ResultType { get; set; }

    /// <summary>
    /// The result tags
    /// </summary>
    public List<string> ResultTags { get; set; }

    /// <summary>
    /// The result value (JSON)
    /// </summary>
    public string ResultValue { get; set; }

    /// <summary>
    /// The result numeric value
    /// </summary>
    public decimal? ResultNumericValue { get; set; }

    /// <summary>
    /// The result culture
    /// </summary>
    public string ResultCulture { get; set; }

    /// <summary>
    /// The result attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <summary>
    /// The payrun job id
    /// </summary>
    public int JobId { get; set; }

    /// <summary>
    /// The payrun job name
    /// </summary>
    public string JobName { get; set; }

    /// <summary>
    /// The payrun job reason
    /// </summary>
    public string JobReason { get; set; }

    /// <summary>
    /// The forecast name (immutable)
    /// </summary>
    public string Forecast { get; set; }

    /// <summary>
    /// The payrun job status
    /// </summary>
    public PayrunJobStatus JobStatus { get; set; }

    /// <summary>
    /// The cycle name (immutable)
    /// </summary>
    public string CycleName { get; set; }

    /// <summary>
    /// The period name (immutable)
    /// </summary>
    public string PeriodName { get; set; }

    /// <summary>
    /// The period start date
    /// </summary>
    public DateTime PeriodStart { get; set; }

    /// <summary>
    /// The period end date
    /// </summary>
    public DateTime PeriodEnd { get; set; }

    /// <summary>
    /// The payrun id
    /// </summary>
    public int PayrunId { get; set; }

    /// <summary>
    /// The payrun name
    /// </summary>
    public string PayrunName { get; set; }

    /// <summary>
    /// The payroll id
    /// </summary>
    public int PayrollId { get; set; }

    /// <summary>
    /// The payroll name
    /// </summary>
    public string PayrollName { get; set; }

    /// <summary>
    /// The division id
    /// </summary>
    public int DivisionId { get; set; }

    /// <summary>
    /// The division name
    /// </summary>
    public string DivisionName { get; set; }

    /// <summary>
    /// The division culture
    /// </summary>
    public string Culture { get; set; }

    /// <summary>
    /// The user id
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// The user identifier
    /// </summary>
    public string UserIdentifier { get; set; }

    /// <summary>
    /// The employee id
    /// </summary>
    public int EmployeeId { get; set; }

    /// <summary>
    /// The employee identifier
    /// </summary>
    public string EmployeeIdentifier { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public PayrollResultValue()
    {
    }

    /// <summary>
    /// Copy constructor
    /// </summary>
    public PayrollResultValue(PayrollResultValue copySource) =>
        CopyTool.CopyProperties(copySource, this);

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(PayrollResultValue compare) =>
        CompareTool.EqualProperties(this, compare);
}