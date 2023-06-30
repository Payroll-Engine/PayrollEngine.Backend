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