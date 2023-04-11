using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Collector result query
/// </summary>
public class CollectorResultQuery : PayrollResultQuery
{
    /// <summary>The payrun job id</summary>
    public int? PayrunJobId { get; set; }

    /// <summary>The The parent job id</summary>
    public int? ParentPayrunJobId { get; set; }

    /// <summary>The collector names</summary>
    public IEnumerable<string> CollectorNames { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public CollectorResultQuery()
    {
    }

    /// <summary>
    /// Copy constructor
    /// </summary>
    public CollectorResultQuery(CollectorResultQuery copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>
    /// Copy constructor
    /// </summary>
    public CollectorResultQuery(PayrollResultQuery copySource) :
        base(copySource)
    {
    }
}