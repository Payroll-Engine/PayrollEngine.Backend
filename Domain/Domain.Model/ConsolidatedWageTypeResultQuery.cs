using System;
using System.Collections.Generic;
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Consolidated wage type result query
/// </summary>
public class ConsolidatedWageTypeResultQuery : WageTypeResultQuery
{
    /// <summary>The period starts</summary>
    public IEnumerable<DateTime> PeriodStarts { get; set; }

    /// <summary>Exclude retro jobs (ParentJobId IS NOT NULL): returns only original main-job values per period</summary>
    public bool NoRetro { get; set; }

    /// <summary>
    /// Exclude retro jobs belonging to a specific parent job (current payrun).
    /// Retro jobs from earlier payruns (different ParentJobId) remain included.
    /// Set by the backend runtime to the current payrun's full-job id; never exposed to the client.
    /// </summary>
    public int? ExcludeParentJobId { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public ConsolidatedWageTypeResultQuery()
    {
    }

    /// <summary>
    /// Copy constructor
    /// </summary>
    public ConsolidatedWageTypeResultQuery(ConsolidatedWageTypeResultQuery copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>
    /// Copy constructor
    /// </summary>
    public ConsolidatedWageTypeResultQuery(WageTypeResultQuery copySource) :
        base(copySource)
    {
    }

    /// <summary>
    /// Copy constructor
    /// </summary>
    public ConsolidatedWageTypeResultQuery(PayrollResultQuery copySource) :
        base(copySource)
    {
    }
}
