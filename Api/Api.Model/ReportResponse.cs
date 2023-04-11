using System.Collections.Generic;
using PayrollEngine.Data;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The report response API object
/// </summary>
public class ReportResponse
{
    /// <summary>
    /// The report parameters
    /// </summary>
    public Dictionary<string, string> Parameters { get; set; }

    /// <summary>
    /// The report queries, key is the query name and value the api operation name
    /// </summary>
    public Dictionary<string, string> Queries { get; set; }

    /// <summary>
    /// The report data relations, based on the queries
    /// </summary>
    public List<DataRelation> Relations { get; set; }

    /// <summary>
    /// The report name
    /// </summary>
    public string ReportName { get; set; }

    /// <summary>
    /// The report language
    /// </summary>
    public Language Language { get; set; }

    /// <summary>
    /// The report user identifier
    /// </summary>
    public string User { get; set; }

    /// <summary>
    /// The report result data, a serialized data set
    /// </summary>
    public DataSet Result { get; set; }
}