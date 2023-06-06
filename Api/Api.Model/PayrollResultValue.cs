using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// Payroll result value
/// </summary>
public class PayrollResultValue
{
    /// <summary>
    /// The payroll result id
    /// </summary>
    public int PayrollResultId { get; set; }

    /// <summary>
    /// The creation date
    /// </summary>
    public DateTime Created { get; set; }

    #region Result

    /// <summary>
    /// The result kind: wage type or collector
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
    /// The kind name, wage type number or collect type
    /// </summary>
    [StringLength(128)]
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
    [StringLength(128)]
    public string ResultValue { get; set; }

    /// <summary>
    /// The result numeric value
    /// </summary>
    public decimal? ResultNumericValue { get; set; }

    /// <summary>
    /// The result attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    #endregion

    #region Job

    /// <summary>
    /// The payrun job id
    /// </summary>
    public int JobId { get; set; }

    /// <summary>
    /// The payrun job name
    /// </summary>
    [StringLength(128)]
    public string JobName { get; set; }

    /// <summary>
    /// The payrun job reason
    /// </summary>
    [StringLength(128)]
    public string JobReason { get; set; }

    /// <summary>
    /// The payrun job status
    /// </summary>
    public PayrunJobStatus JobStatus { get; set; }

    /// <summary>
    /// The forecast name (immutable)
    /// </summary>
    public string Forecast { get; set; }

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

    #endregion

    #region Payrun

    /// <summary>
    /// The payrun id
    /// </summary>
    public int PayrunId { get; set; }

    /// <summary>
    /// The payrun name
    /// </summary>
    [StringLength(128)]
    public string PayrunName { get; set; }

    #endregion

    #region Payroll

    /// <summary>
    /// The payroll id
    /// </summary>
    public int PayrollId { get; set; }

    /// <summary>
    /// The payroll name
    /// </summary>
    [StringLength(128)]
    public string PayrollName { get; set; }

    #endregion

    #region Division

    /// <summary>
    /// The division id
    /// </summary>
    public int DivisionId { get; set; }

    /// <summary>
    /// The division name
    /// </summary>
    [StringLength(128)]
    public string DivisionName { get; set; }

    /// <summary>
    /// The division culture
    /// </summary>
    [StringLength(128)]
    public string Culture { get; set; }

    #endregion

    #region User

    /// <summary>
    /// The user id
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// The user identifier
    /// </summary>
    [StringLength(128)]
    public string UserIdentifier { get; set; }

    #endregion

    #region Employee

    /// <summary>
    /// The employee id
    /// </summary>
    public int EmployeeId { get; set; }

    /// <summary>
    /// The employee identifier
    /// </summary>
    [StringLength(128)]
    public string EmployeeIdentifier { get; set; }

    #endregion
}