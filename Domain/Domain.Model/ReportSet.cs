using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A report set 
/// </summary>
public class ReportSet : Report, IEquatable<ReportSet>
{
    /// <summary>
    /// The regulation id
    /// </summary>
    public int RegulationId { get; set; }

    /// <summary>
    /// The report parameters
    /// </summary>
    public List<ReportParameter> Parameters { get; set; }

    /// <summary>
    /// The report templates
    /// </summary>
    public List<ReportTemplate> Templates { get; set; }

    /// <inheritdoc/>
    public ReportSet()
    {
    }

    /// <inheritdoc/>
    public ReportSet(ReportSet copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(ReportSet compare) =>
        CompareTool.EqualProperties(this, compare);
}