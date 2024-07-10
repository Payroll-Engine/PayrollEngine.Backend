using System;
using System.Collections.Generic;
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A consolidated payroll result
/// </summary>
public class ConsolidatedPayrollResult : IEquatable<ConsolidatedPayrollResult>
{
    /// <summary>
    /// The wage type results
    /// </summary>
    public List<WageTypeResultSet> WageTypeResults { get; set; } = [];

    /// <summary>
    /// The collector results
    /// </summary>
    public List<CollectorResult> CollectorResults { get; set; } = [];

    /// <summary>
    /// The payrun results
    /// </summary>
    public List<PayrunResult> PayrunResults { get; set; } = [];

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(ConsolidatedPayrollResult compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override string ToString() =>
        $"{WageTypeResults?.Count} wage types, {CollectorResults?.Count} collectors, {PayrunResults?.Count} case values {base.ToString()}";
}