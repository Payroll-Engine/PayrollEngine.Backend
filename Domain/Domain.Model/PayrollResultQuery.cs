using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Payroll result query
/// </summary>
public class PayrollResultQuery
{
    /// <summary>The tenant id</summary>
    [Required]
    public int TenantId { get; set; }

    /// <summary>The employee id</summary>
    [Required]
    public int EmployeeId { get; set; }
    
    /// <summary>The division id</summary>
    public int? DivisionId { get; set; }
    
    /// <summary>The period</summary>
    [Required]
    public DatePeriod Period { get; set; }
    
    /// <summary>The forecast name</summary>
    public string Forecast { get; set; }
    
    /// <summary>The result tags</summary>
    public IEnumerable<string> Tags { get; set; }
    
    /// <summary>The payrun job status</summary>
    public PayrunJobStatus? JobStatus { get; set; }
    
    /// <summary>The evaluation date (default: UTC now)</summary>
    public DateTime? EvaluationDate { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public PayrollResultQuery()
    {
    }

    /// <summary>
    /// Copy constructor
    /// </summary>
    public PayrollResultQuery(PayrollResultQuery copySource) =>
        CopyTool.CopyProperties(copySource, this);
}