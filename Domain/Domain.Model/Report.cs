using System;
using System.Collections.Generic;
using System.Linq;
using PayrollEngine.Data;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A payroll report
/// </summary>
public class Report : ScriptTrackDomainObject<ReportAudit>, IDerivableObject, IClusterObject,
    INamedObject, IDomainAttributeObject, IEquatable<Report>
{
    private static readonly List<FunctionType> FunctionTypes = new()
    {
        FunctionType.ReportBuild,
        FunctionType.ReportStart,
        FunctionType.ReportEnd
    };

    private static readonly IEnumerable<string> EmbeddedScriptNames = new[]
    {
        "Report\\Report.cs",
        "Function\\ReportFunction.cs"
    };

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
    public Report()
    {
    }

    /// <inheritdoc/>
    public Report(Report copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(Report compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override ReportAudit ToAuditObject()
    {
        if (!this.HasId())
        {
            return null;
        }
        return new()
        {
            ReportId = Id,
            Name = Name,
            NameLocalizations = NameLocalizations,
            Description = Description,
            DescriptionLocalizations = DescriptionLocalizations,
            Category = Category,
            AttributeMode = AttributeMode,
            Queries = Queries,
            Relations = Relations,
            BuildExpression = BuildExpression,
            StartExpression = StartExpression,
            EndExpression = EndExpression,
            Attributes = Attributes,
            Clusters = Clusters,
        };
    }

    /// <inheritdoc/>
    public override void FromAuditObject(ReportAudit audit)
    {
        base.FromAuditObject(audit);

        Id = audit.ReportId;
        Name = audit.Name;
        NameLocalizations = audit.NameLocalizations.Copy();
        Description = audit.Description;
        DescriptionLocalizations = audit.DescriptionLocalizations.Copy();
        Category = audit.Category;
        AttributeMode = audit.AttributeMode;
        Queries = audit.Queries.Copy();
        Relations = audit.Relations.Copy();
        BuildExpression = audit.BuildExpression;
        StartExpression = audit.StartExpression;
        EndExpression = audit.EndExpression;
        Attributes = audit.Attributes.Copy();
        Clusters = audit.Clusters.Copy();
    }

    #region Scripting

    /// <inheritdoc/>
    public override bool HasExpression =>
        GetFunctionScripts().Values.Any(x => !string.IsNullOrWhiteSpace(x));

    /// <inheritdoc/>
    public override bool HasObjectScripts => true;

    /// <inheritdoc/>
    public override List<FunctionType> GetFunctionTypes() => FunctionTypes;

    /// <inheritdoc/>
    public override IDictionary<FunctionType, string> GetFunctionScripts()
    {
        var scripts = new Dictionary<FunctionType, string>();
        if (!string.IsNullOrWhiteSpace(BuildExpression))
        {
            scripts.Add(FunctionType.ReportBuild, BuildExpression);
        }
        if (!string.IsNullOrWhiteSpace(StartExpression))
        {
            scripts.Add(FunctionType.ReportStart, StartExpression);
        }
        if (!string.IsNullOrWhiteSpace(EndExpression))
        {
            scripts.Add(FunctionType.ReportEnd, EndExpression);
        }
        return scripts;
    }

    /// <inheritdoc/>
    public override IEnumerable<string> GetEmbeddedScriptNames() =>
        EmbeddedScriptNames;

    #endregion

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Name} {base.ToString()}";
}