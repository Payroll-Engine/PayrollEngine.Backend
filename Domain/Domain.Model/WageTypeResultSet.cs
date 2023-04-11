using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A wage type result set
/// </summary>
public class WageTypeResultSet : WageTypeResult, IEquatable<WageTypeResultSet>
{
    /// <summary>
    /// The wage type custom results (immutable)
    /// </summary>
    public List<WageTypeCustomResult> CustomResults { get; set; }

    /// <inheritdoc/>
    public WageTypeResultSet()
    {
    }

    /// <inheritdoc/>
    public WageTypeResultSet(WageTypeResult copySource) :
        base(copySource)
    {
    }

    /// <inheritdoc/>
    public WageTypeResultSet(WageTypeResultSet copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(WageTypeResultSet compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override string ToString() =>
        $"{CustomResults?.Count} custom results {base.ToString()}";
}