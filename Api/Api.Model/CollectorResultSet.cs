
namespace PayrollEngine.Api.Model;

/// <summary>
/// The collector result set API object
/// </summary>
public class CollectorResultSet : CollectorResult
{
    /// <summary>
    /// The collector custom results (immutable)
    /// </summary>
    public CollectorCustomResult[] CustomResults { get; set; }
}