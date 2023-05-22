using System.Collections.Generic;
using PayrollEngine.Data;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A payroll report audit
/// </summary>
public class ReportAudit : ScriptAuditDomainObject, IDomainAttributeObject
{
    /// <summary>
    /// The report id
    /// </summary>
    public int ReportId { get; set; }

    /// <summary>
    /// The payroll result report name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The localized payroll result report names
    /// </summary>
    public Dictionary<string, string> NameLocalizations { get; set; }

    /// <summary>
    /// The payroll result report description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The localized payroll result report descriptions
    /// </summary>
    public Dictionary<string, string> DescriptionLocalizations { get; set; }
    
    /// <summary>
    /// The override type
    /// </summary>
    public OverrideType OverrideType { get; set; }

    /// <summary>
    /// The report category
    /// </summary>
    public string Category { get; set; }

    /// <summary>
    /// The report attribute mode
    /// </summary>
    public ReportAttributeMode AttributeMode { get; set; }

    /// <summary>
    /// The report queries, key is the query name and value the api operation name
    /// </summary>
    public Dictionary<string, string> Queries { get; set; }

    /// <summary>
    /// The report data relations, based on the queries
    /// </summary>
    public List<DataRelation> Relations { get; set; }

    /// <summary>
    /// The report build expression
    /// </summary>
    public string BuildExpression { get; set; }

    /// <summary>
    /// The report start expression
    /// </summary>
    public string StartExpression { get; set; }

    /// <summary>
    /// The report end expression
    /// </summary>
    public string EndExpression { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <summary>
    /// The wage type clusters
    /// </summary>
    public List<string> Clusters { get; set; }

    /// <inheritdoc/>
    public ReportAudit()
    {
    }

    /// <inheritdoc/>
    public ReportAudit(ReportAudit copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }
}