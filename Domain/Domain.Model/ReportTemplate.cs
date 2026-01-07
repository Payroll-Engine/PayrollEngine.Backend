using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A payroll report template
/// </summary>
public class ReportTemplate : TrackDomainObject<ReportTemplateAudit>, IDomainAttributeObject,
    INamespaceObject, IDerivableObject, IEquatable<ReportTemplate>
{
    /// <summary>
    /// The report template name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The report culture
    /// </summary>
    public string Culture { get; set; }

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
    /// The override type
    /// </summary>
    public OverrideType OverrideType { get; set; }

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

    /// <inheritdoc/>
    public void ApplyNamespace(string @namespace)
    {
        Name = Name.EnsureNamespace(@namespace);
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
            Name = Name,
            Culture = Culture,
            Content = Content,
            ContentType = ContentType,
            Schema = Schema,
            Resource = Resource,
            Attributes = Attributes,
            OverrideType = OverrideType
        };
    }

    /// <inheritdoc/>
    public override void FromAuditObject(ReportTemplateAudit audit)
    {
        base.FromAuditObject(audit);

        Id = audit.ReportTemplateId;
        Name = audit.Name;
        Culture = audit.Culture;
        Content = audit.Content;
        ContentType = audit.ContentType;
        Schema = audit.Schema;
        Resource = audit.Resource;
        OverrideType = audit.OverrideType;
        Attributes = audit.Attributes.Copy();
    }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Name} ({Culture}) {base.ToString()}";
}