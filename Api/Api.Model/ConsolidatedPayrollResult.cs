// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace PayrollEngine.Api.Model;

/// <summary>
/// A consolidated payroll result
/// </summary>
public class ConsolidatedPayrollResult
{
    /// <summary>
    /// The wage type results
    /// </summary>
    public WageTypeResultSet[] WageTypeResults { get; set; }

    /// <summary>
    /// The collector results
    /// </summary>
    public CollectorResult[] CollectorResults { get; set; }

    /// <summary>
    /// The payrun results
    /// </summary>
    public PayrunResult[] PayrunResults { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{WageTypeResults?.Length} wage types, {CollectorResults?.Length} collectors, {PayrunResults?.Length} payruns {base.ToString()}";
}