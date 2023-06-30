using System;
using System.Collections.Generic;
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Consolidated collector result query
/// </summary>
public class ConsolidatedCollectorResultQuery : CollectorResultQuery
{
    /// <summary>The period starts</summary>
    public IEnumerable<DateTime> PeriodStarts { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public ConsolidatedCollectorResultQuery()
    {
    }

    /// <summary>
    /// Copy constructor
    /// </summary>
    public ConsolidatedCollectorResultQuery(ConsolidatedCollectorResultQuery copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>
    /// Copy constructor
    /// </summary>
    public ConsolidatedCollectorResultQuery(CollectorResultQuery copySource) :
        base(copySource)
    {
    }

    /// <summary>
    /// Copy constructor
    /// </summary>
    public ConsolidatedCollectorResultQuery(PayrollResultQuery copySource) :
        base(copySource)
    {
    }
}