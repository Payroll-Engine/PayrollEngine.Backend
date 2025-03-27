
namespace PayrollEngine.Api.Model;

/// <summary>
/// The payroll result set API object
/// </summary>
// ReSharper disable CollectionNeverQueried.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
public class PayrollResultSet : PayrollResult
{
    /// <summary>
    /// The wage type results
    /// </summary>
    public WageTypeResultSet[] WageTypeResults { get; set; }

    /// <summary>
    /// The collector results
    /// </summary>
    public CollectorResultSet[] CollectorResults { get; set; }

    /// <summary>
    /// The payrun results
    /// </summary>
    public PayrunResult[] PayrunResults { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{WageTypeResults?.Length} wage types, {CollectorResults?.Length} collectors {base.ToString()}";
}