using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Payrun result query
/// </summary>
public class PayrunResultQuery : PayrollResultQuery
{
    /// <summary>The result names</summary>
    public IEnumerable<string> ResultNames { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public PayrunResultQuery()
    {
    }

    /// <summary>
    /// Copy constructor
    /// </summary>
    public PayrunResultQuery(PayrunResultQuery copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>
    /// Copy constructor
    /// </summary>
    public PayrunResultQuery(PayrollResultQuery copySource) :
        base(copySource)
    {
    }
}