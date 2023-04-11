
namespace PayrollEngine.Api.Model;

/// <summary>
/// The wage type result set API object
/// </summary>
public class WageTypeResultSet : WageTypeResult
{
    /// <summary>
    /// The wage type custom results (immutable)
    /// </summary>
    public WageTypeCustomResult[] CustomResults { get; set; }
}