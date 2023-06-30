using System.Collections.Generic;
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A payroll report template
/// </summary>
public class ReportTemplateAudit : ScriptAuditDomainObject, IDomainAttributeObject
{
    /// <summary>
    /// The report template id
    /// </summary>
    public int ReportTemplateId { get; set; }

    /// <summary>
    /// The report template name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The report culture
    /// </summary>
    public string Culture { get; set; }

    /// <summary>
    /// The report content
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// The report content type
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// The report schema
    /// </summary>
    public string Schema { get; set; }

    /// <summary>
    /// The report external resource
    /// </summary>
    public string Resource { get; set; }

    /// <summary>
    /// The override type
    /// </summary>
    public OverrideType OverrideType { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportTemplateAudit"/> class
    /// </summary>
    public ReportTemplateAudit()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportTemplateAudit"/> class
    /// </summary>
    /// <param name="copySource">The copy source.</param>
    public ReportTemplateAudit(ReportTemplateAudit copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }
}