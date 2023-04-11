using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A payroll report template
/// </summary>
public class ReportTemplate : TrackDomainObject<ReportTemplateAudit>, IDomainAttributeObject, IEquatable<ReportTemplate>
{
    /// <summary>
    /// The report language
    /// </summary>
    public Language Language { get; set; }

    /// <summary>
    /// The report content (client owned)
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// The report content type
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// The report schema (client owned)
    /// </summary>
    public string Schema { get; set; }

    /// <summary>
    /// The report external resource
    /// </summary>
    public string Resource { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportTemplate"/> class
    /// </summary>
    public ReportTemplate()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportTemplate"/> class
    /// </summary>
    /// <param name="copySource">The copy source.</param>
    public ReportTemplate(ReportTemplate copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(ReportTemplate compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override ReportTemplateAudit ToAuditObject()
    {
        if (!this.HasId())
        {
            return null;
        }
        return new()
        {
            ReportTemplateId = Id,
            Language = Language,
            Content = Content,
            ContentType = ContentType,
            Schema = Schema,
            Resource = Resource,
            Attributes = Attributes,
        };
    }

    /// <inheritdoc/>
    public override void FromAuditObject(ReportTemplateAudit audit)
    {
        base.FromAuditObject(audit);

        Id = audit.ReportTemplateId;
        Language = audit.Language;
        Content = audit.Content;
        ContentType = audit.ContentType;
        Schema = audit.Schema;
        Resource = audit.Resource;
        Attributes = audit.Attributes.Copy();
    }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Language} {base.ToString()}";
}