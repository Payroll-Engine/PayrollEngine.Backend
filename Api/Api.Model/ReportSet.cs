
namespace PayrollEngine.Api.Model;

/// <summary>
/// The report set API object
/// </summary>
public class ReportSet : Report
{
    /// <summary>
    /// The regulation id
    /// </summary>
    public int RegulationId { get; set; }

    /// <summary>
    /// The report parameters
    /// </summary>
    public ReportParameter[] Parameters { get; set; }

    /// <summary>
    /// The report templates
    /// </summary>
    public ReportTemplate[] Templates { get; set; }
}