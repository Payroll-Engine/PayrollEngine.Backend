using System;
using System.Collections.Generic;
using PayrollEngine.Data;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A payroll report response
/// </summary>
public class ReportResponse : IEquatable<ReportResponse>
{
    /// <summary>
    /// The report queries, key is the query name and value the api operation name
    /// </summary>
    public Dictionary<string, string> Queries { get; set; }

    /// <summary>
    /// The report relations
    /// </summary>
    public List<DataRelation> Relations { get; set; }

    /// <summary>
    /// The report parameters
    /// </summary>
    public Dictionary<string, string> Parameters { get; set; }

    /// <summary>
    /// The report name
    /// </summary>
    public string ReportName { get; set; }

    /// <summary>
    /// The report culture
    /// </summary>
    public string Culture { get; set; }

    /// <summary>
    /// The report user identifier
    /// </summary>
    public string User { get; set; }

    /// <summary>
    /// The report result data
    /// </summary>
    public DataSet Result { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportResponse"/> class
    /// </summary>
    public ReportResponse()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportResponse"/> class
    /// </summary>
    /// <param name="copySource">The copy source.</param>
    public ReportResponse(ReportResponse copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(ReportResponse compare) =>
        CompareTool.EqualProperties(this, compare);
}