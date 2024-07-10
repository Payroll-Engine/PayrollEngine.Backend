using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A payroll result
/// </summary>
public class PayrollResultSet : PayrollResult, IEquatable<PayrollResultSet>
{
    /// <summary>
    /// The wage type results
    /// </summary>
    public List<WageTypeResultSet> WageTypeResults { get; set; } = [];

    /// <summary>
    /// The collector results
    /// </summary>
    public List<CollectorResultSet> CollectorResults { get; set; } = [];

    /// <summary>
    /// The payrun results
    /// </summary>
    public List<PayrunResult> PayrunResults { get; set; } = [];

    /// <inheritdoc/>
    public PayrollResultSet()
    {
    }

    /// <inheritdoc/>
    public PayrollResultSet(PayrollResultSet copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(PayrollResultSet compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override string ToString() =>
        $"{WageTypeResults?.Count} wage types, {CollectorResults?.Count} collectors {base.ToString()}";
}