using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Consolidated payrun result query
/// </summary>
public class ConsolidatedPayrunResultQuery : PayrunResultQuery
{
    /// <summary>The period starts</summary>
    public IEnumerable<DateTime> PeriodStarts { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public ConsolidatedPayrunResultQuery()
    {
    }

    /// <summary>
    /// Copy constructor
    /// </summary>
    public ConsolidatedPayrunResultQuery(ConsolidatedPayrunResultQuery copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>
    /// Copy constructor
    /// </summary>
    public ConsolidatedPayrunResultQuery(PayrunResultQuery copySource) :
        base(copySource)
    {
    }

    /// <summary>
    /// Copy constructor
    /// </summary>
    public ConsolidatedPayrunResultQuery(PayrollResultQuery copySource) :
        base(copySource)
    {
    }
}