using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Wage type result query
/// </summary>
public class WageTypeResultQuery : PayrollResultQuery
{
    /// <summary>The wage type numbers</summary>
    public IEnumerable<decimal> WageTypeNumbers { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public WageTypeResultQuery()
    {
    }

    /// <summary>
    /// Copy constructor
    /// </summary>
    public WageTypeResultQuery(WageTypeResultQuery copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>
    /// Copy constructor
    /// </summary>
    public WageTypeResultQuery(PayrollResultQuery copySource) :
        base(copySource)
    {
    }
}