using System.Collections.Generic;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The report request API object
/// </summary>
public class ReportRequest
{
    /// <summary>
    /// The report language
    /// </summary>
    public Language Language { get; set; }
        
    /// <summary>
    /// The report user
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// The report parameters
    /// </summary>
    public Dictionary<string, string> Parameters { get; set; }
}