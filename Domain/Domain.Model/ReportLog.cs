using System;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A payroll report log
/// </summary>
public class ReportLog : DomainObjectBase, IEquatable<ReportLog>
{
    /// <summary>
    /// The report name (immutable)
    /// </summary>
    public string ReportName { get; set; }

    /// <summary>
    /// The report date (immutable)
    /// </summary>
    public DateTime ReportDate { get; set; }

    /// <summary>
    /// The report log key (immutable)
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// The report user (immutable)
    /// </summary>
    public string User { get; set; }

    /// <summary>
    /// The report message (immutable)
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportLog"/> class
    /// </summary>
    public ReportLog()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportLog"/> class
    /// </summary>
    /// <param name="copySource">The copy source.</param>
    public ReportLog(ReportLog copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(ReportLog compare) =>
        CompareTool.EqualProperties(this, compare);
}