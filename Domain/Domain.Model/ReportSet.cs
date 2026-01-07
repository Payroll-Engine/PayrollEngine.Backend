using System;
using System.Collections.Generic;
// ReSharper disable UnusedAutoPropertyAccessor.Global

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

    /// <inheritdoc/>
    public override void ApplyNamespace(string @namespace)
    {
        base.ApplyNamespace(@namespace);
        if (Parameters != null)
        {
            foreach (var parameter in Parameters)
            {
                parameter.ApplyNamespace(@namespace);
            }
        }
        if (Templates != null)
        {
            foreach (var template in Templates)
            {
                template.ApplyNamespace(@namespace);
            }
        }
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(ReportSet compare) =>
        CompareTool.EqualProperties(this, compare);
}