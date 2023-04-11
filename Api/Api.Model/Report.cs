using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PayrollEngine.Data;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The report API object
/// </summary>
public class Report : ApiObjectBase
{
    /// <summary>
    /// The payroll result report name
    /// </summary>
    [Required]
    [StringLength(128)]
    public string Name { get; set; }

    /// <summary>
    /// The localized payroll result report names
    /// </summary>
    [Localization(nameof(Name))]
    public Dictionary<string, string> NameLocalizations { get; set; }

    /// <summary>
    /// The payroll result report description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The localized payroll result report descriptions
    /// </summary>
    [Localization(nameof(Description))]
    public Dictionary<string, string> DescriptionLocalizations { get; set; }

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
    public override string ToString() =>
        $"{Name} {base.ToString()}";
}