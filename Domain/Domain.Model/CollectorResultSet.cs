using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A collector result set
/// </summary>
public class CollectorResultSet : CollectorResult, IEquatable<CollectorResultSet>
{
    /// <summary>
    /// The collector custom results (immutable)
    /// </summary>
    public List<CollectorCustomResult> CustomResults { get; set; }

    /// <inheritdoc/>
    public CollectorResultSet()
    {
    }

    /// <inheritdoc/>
    public CollectorResultSet(CollectorResult copySource) :
        base(copySource)
    {
    }

    /// <inheritdoc/>
    public CollectorResultSet(CollectorResultSet copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(CollectorResultSet compare) =>
        CompareTool.EqualProperties(this, compare);
}