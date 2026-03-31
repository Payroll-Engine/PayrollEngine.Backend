using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A payrun job with its payroll result sets — used for archive restore and migration.
/// Carries denormalized name fields so all FK ids can be resolved
/// in the target system independently of the source system's ids.
/// Only jobs with <see cref="PayrunJobStatus.Complete"/> or <see cref="PayrunJobStatus.Forecast"/> are accepted on import.
/// </summary>
public class PayrunJobSet : PayrunJob
{
    /// <summary>The payrun name (denormalized, used for import id resolution)</summary>
    public string PayrunName { get; set; }

    /// <summary>The division name (denormalized, used for import id resolution)</summary>
    public string DivisionName { get; set; }

    /// <summary>The creating user identifier (denormalized, used for import id resolution)</summary>
    public string UserIdentifier { get; set; }

    /// <summary>The payroll result sets belonging to this job</summary>
    public List<PayrollResultSet> ResultSets { get; set; }
}
