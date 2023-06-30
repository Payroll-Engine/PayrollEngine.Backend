using System;
using System.Collections.Generic;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A payroll report request
/// </summary>
public class ReportRequest : IEquatable<ReportRequest>
{
    /// <summary>
    /// The report culture
    /// </summary>
    public string Culture { get; set; }

    /// <summary>
    /// The report user
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// The report parameters
    /// </summary>
    public Dictionary<string, string> Parameters { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportRequest"/> class
    /// </summary>
    public ReportRequest()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportRequest"/> class
    /// </summary>
    /// <param name="copySource">The copy source.</param>
    public ReportRequest(ReportRequest copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(ReportRequest compare) =>
        CompareTool.EqualProperties(this, compare);
}