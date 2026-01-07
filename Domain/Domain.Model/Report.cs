using System;
using System.Collections.Generic;
using PayrollEngine.Data;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A payroll report
/// </summary>
public class Report : ScriptTrackDomainObject<ReportAudit>, IDerivableObject, IClusterObject,
    INamedObject, INamespaceObject, IDomainAttributeObject, IEquatable<Report>
{
    private static readonly List<FunctionType> FunctionTypes =
    [
        FunctionType.ReportBuild,
        FunctionType.ReportStart,
        FunctionType.ReportEnd
    ];

    private static readonly IEnumerable<string> EmbeddedScriptNames =
    [
        "Report\\Report.cs",
        "Function\\ReportFunction.cs"
    ];

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
    /// The user type (default: employee)
    /// </summary>
    public UserType UserType { get; set; }

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
    protected Report(Report copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <inheritdoc/>
    public virtual void ApplyNamespace(string @namespace)
    {
        Name = Name.EnsureNamespace(@namespace);
        Clusters = Clusters.EnsureNamespace(@namespace);
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
            OverrideType = OverrideType,
            Category = Category,
            AttributeMode = AttributeMode,
            UserType = UserType,
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
        OverrideType = audit.OverrideType;
        Category = audit.Category;
        AttributeMode = audit.AttributeMode;
        UserType = audit.UserType;
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
    public override bool HasAnyExpression =>
        !string.IsNullOrWhiteSpace(BuildExpression) ||
        !string.IsNullOrWhiteSpace(StartExpression) ||
        !string.IsNullOrWhiteSpace(EndExpression);

    /// <inheritdoc/>
    public override bool HasAnyAction => false;

    /// <inheritdoc/>
    public override bool HasObjectScripts => true;

    /// <inheritdoc/>
    public override List<FunctionType> GetFunctionTypes() => FunctionTypes;

    /// <inheritdoc/>
    public override string GetFunctionScript(FunctionType functionType) =>
        functionType switch
        {
            FunctionType.ReportBuild => BuildExpression,
            FunctionType.ReportStart => StartExpression,
            FunctionType.ReportEnd => EndExpression,
            _ => null
        };

    /// <inheritdoc/>
    public override List<string> GetFunctionActions(FunctionType functionType) => null;

    /// <inheritdoc/>
    public override IEnumerable<string> GetEmbeddedScriptNames() =>
        EmbeddedScriptNames;

    #endregion

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Name} {base.ToString()}";
}