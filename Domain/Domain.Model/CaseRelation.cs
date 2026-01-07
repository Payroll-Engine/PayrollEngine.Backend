using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A case field used in national, company and employee
/// </summary>
public class CaseRelation : ScriptTrackDomainObject<CaseRelationAudit>, IDerivableObject,
    INamespaceObject, IClusterObject, IDomainAttributeObject, IEquatable<CaseRelation>
{
    private static readonly List<FunctionType> FunctionTypes =
    [
        FunctionType.CaseRelationBuild,
        FunctionType.CaseRelationValidate
    ];

    /// <summary>
    /// The relation source case name (immutable)
    /// </summary>
    public string SourceCaseName { get; set; }

    /// <summary>
    /// The localized source case names
    /// </summary>
    public Dictionary<string, string> SourceCaseNameLocalizations { get; set; }

    /// <summary>
    /// The relation source case slot
    /// </summary>
    public string SourceCaseSlot { get; set; }

    /// <summary>
    /// The localized source case slots
    /// </summary>
    public Dictionary<string, string> SourceCaseSlotLocalizations { get; set; }

    /// <summary>
    /// The relation target case name (immutable)
    /// </summary>
    public string TargetCaseName { get; set; }

    /// <summary>
    /// The localized target case names
    /// </summary>
    public Dictionary<string, string> TargetCaseNameLocalizations { get; set; }

    /// <summary>
    /// The relation target case slot
    /// </summary>
    public string TargetCaseSlot { get; set; }

    /// <summary>
    /// The localized target case slots
    /// </summary>
    public Dictionary<string, string> TargetCaseSlotLocalizations { get; set; }

    /// <summary>
    /// The relation key hash code
    /// The hash is used by database indexes
    /// </summary>
    public int RelationHash
    {
        get => $"{SourceCaseName}{SourceCaseSlot}{TargetCaseName}{TargetCaseSlot}".ToPayrollHash();
        // ReSharper disable once ValueParameterNotUsed
        set
        {
            // hash is calculated
        }
    }

    /// <summary>
    /// The expression used to build the case relation
    /// </summary>
    public string BuildExpression { get; set; }

    /// <summary>
    /// The expression which evaluates if the case relation is valid
    /// </summary>
    public string ValidateExpression { get; set; }

    /// <inheritdoc/>
    public OverrideType OverrideType { get; set; }

    /// <summary>
    /// The case relation order
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// The case relation build actions
    /// </summary>
    public List<string> BuildActions { get; set; }

    /// <summary>
    /// The case relation validate actions
    /// </summary>
    public List<string> ValidateActions { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <summary>
    /// The case relation clusters
    /// </summary>
    public List<string> Clusters { get; set; }

    /// <inheritdoc/>
    public CaseRelation()
    {
    }

    /// <inheritdoc/>
    protected CaseRelation(CaseRelation copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <inheritdoc/>
    public void ApplyNamespace(string @namespace)
    {
        SourceCaseName = SourceCaseName.EnsureNamespace(@namespace);
        TargetCaseName = TargetCaseName.EnsureNamespace(@namespace);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(CaseRelation compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override CaseRelationAudit ToAuditObject()
    {
        if (!this.HasId())
        {
            return null;
        }
        return new()
        {
            CaseRelationId = Id,
            SourceCaseName = SourceCaseName,
            SourceCaseNameLocalizations = SourceCaseNameLocalizations,
            SourceCaseSlot = SourceCaseSlot,
            SourceCaseSlotLocalizations = SourceCaseSlotLocalizations,
            TargetCaseName = TargetCaseName,
            TargetCaseNameLocalizations = TargetCaseNameLocalizations,
            TargetCaseSlot = TargetCaseSlot,
            TargetCaseSlotLocalizations = TargetCaseSlotLocalizations,
            RelationHash = RelationHash,
            BuildExpression = BuildExpression,
            ValidateExpression = ValidateExpression,
            OverrideType = OverrideType,
            Order = Order,
            Script = Script,
            ScriptVersion = ScriptVersion,
            Binary = Binary,
            ScriptHash = ScriptHash,
            BuildActions = BuildActions,
            ValidateActions = ValidateActions,
            Attributes = Attributes,
            Clusters = Clusters
        };
    }

    /// <inheritdoc/>
    public override void FromAuditObject(CaseRelationAudit audit)
    {
        base.FromAuditObject(audit);

        Id = audit.CaseRelationId;
        SourceCaseName = audit.SourceCaseName;
        SourceCaseNameLocalizations = audit.SourceCaseNameLocalizations;
        SourceCaseSlot = audit.SourceCaseSlot;
        SourceCaseSlotLocalizations = audit.SourceCaseSlotLocalizations;
        TargetCaseName = audit.TargetCaseName;
        TargetCaseNameLocalizations = audit.TargetCaseNameLocalizations;
        TargetCaseSlot = audit.TargetCaseSlot;
        TargetCaseSlotLocalizations = audit.TargetCaseSlotLocalizations;
        RelationHash = audit.RelationHash;
        BuildExpression = audit.BuildExpression;
        ValidateExpression = audit.ValidateExpression;
        OverrideType = audit.OverrideType;
        Order = audit.Order;
        BuildActions = audit.BuildActions;
        ValidateActions = audit.ValidateActions;
        Attributes = audit.Attributes;
        Clusters = audit.Clusters;
    }

    #region Scripting

    /// <summary>
    /// Test for build script
    /// </summary>
    public string BuildScript =>
        HasBuildScript ? BuildExpression ?? "true" : null;

    /// <summary>
    /// Test for validate script
    /// </summary>
    public string ValidateScript =>
        HasValidateScript ? ValidateExpression ?? "true" : null;

    /// <summary>
    /// Test for build script
    /// </summary>
    private bool HasBuildScript =>
        AnyExpressionOrActions(BuildExpression, BuildActions);

    /// <summary>
    /// Test for validate script
    /// </summary>
    private bool HasValidateScript =>
        AnyExpressionOrActions(ValidateExpression, ValidateActions);

    /// <inheritdoc/>
    public override bool HasAnyExpression =>
        HasBuildScript ||
        HasValidateScript;

    /// <inheritdoc/>
    public override bool HasAnyAction =>
        AnyActions(BuildActions) ||
        AnyActions(ValidateActions);

    /// <inheritdoc/>
    public override bool HasObjectScripts => true;

    /// <inheritdoc/>
    public override List<FunctionType> GetFunctionTypes() => FunctionTypes;

    /// <inheritdoc/>
    public override string GetFunctionScript(FunctionType functionType) =>
        functionType switch
        {
            FunctionType.CaseRelationBuild => BuildExpression,
            FunctionType.CaseRelationValidate => ValidateExpression,
            _ => null
        };

    /// <inheritdoc/>
    public override List<string> GetFunctionActions(FunctionType functionType) =>
        functionType switch
        {
            FunctionType.CaseRelationBuild => BuildActions,
            FunctionType.CaseRelationValidate => ValidateActions,
            _ => null
        };

    /// <inheritdoc/>
    public override IEnumerable<string> GetEmbeddedScriptNames() =>
        GetEmbeddedScriptNames([
            new(BuildExpression, BuildActions, FunctionType.CaseRelationBuild),
            new(ValidateExpression, ValidateActions, FunctionType.CaseRelationValidate)
        ]);

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
        var sourceCase = string.IsNullOrWhiteSpace(SourceCaseSlot)
            ? SourceCaseName
            : $"{SourceCaseName}.{SourceCaseSlot}";
        var targetCase = string.IsNullOrWhiteSpace(TargetCaseSlot)
            ? TargetCaseName
            : $"{TargetCaseName}.{TargetCaseSlot}";
        return $"{sourceCase} > {targetCase}";
    }
}