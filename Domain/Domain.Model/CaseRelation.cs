using System;
using System.Collections.Generic;
using System.Linq;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A case field used in national, company and employee
/// </summary>
public class CaseRelation : ScriptTrackDomainObject<CaseRelationAudit>, IDerivableObject,
    IClusterObject, IDomainAttributeObject, IEquatable<CaseRelation>
{
    private static readonly List<FunctionType> FunctionTypes = new()
    {
        FunctionType.CaseRelationBuild,
        FunctionType.CaseRelationValidate
    };

    // scripts
    private const string CaseRelationFunctionScript = "Function\\CaseRelationFunction.cs";
    private const string CaseActionScript = "Function\\CaseAction.cs";
    private const string CaseRelationBuildActionsScript = "Function\\CaseRelationBuildActions.cs";
    private const string CaseRelationValidateActionsScript = "Function\\CaseRelationValidateActions.cs";

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
    public string BuildScript
    {
        get
        {
            if (string.IsNullOrWhiteSpace(BuildExpression) &&
                BuildActions != null && BuildActions.Any())
            {
                return "true";
            }
            return BuildExpression;
        }
    }

    /// <summary>
    /// Test for validate script
    /// </summary>
    public string ValidateScript
    {
        get
        {
            if (string.IsNullOrWhiteSpace(ValidateExpression) &&
                ValidateActions != null && ValidateActions.Any())
            {
                return "true";
            }
            return ValidateExpression;
        }
    }

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

        // case relation build
        var buildScript = BuildScript;
        if (!string.IsNullOrWhiteSpace(buildScript))
        {
            scripts.Add(FunctionType.CaseRelationBuild, buildScript);
        }

        // case relation validate
        var validateScript = ValidateScript;
        if (!string.IsNullOrWhiteSpace(validateScript))
        {
            scripts.Add(FunctionType.CaseRelationValidate, validateScript);
        }
        return scripts;
    }

    /// <inheritdoc/>
    public override IEnumerable<string> GetEmbeddedScriptNames()
    {
        // case relation scripts
        var scripts = new List<string>
        {
            CaseRelationFunctionScript,
            CaseActionScript
        };
        // case relation build and validate
        if (!string.IsNullOrWhiteSpace(BuildScript) || !string.IsNullOrWhiteSpace(ValidateScript))
        {
            scripts.Add(CaseRelationBuildActionsScript);
            scripts.Add(CaseRelationValidateActionsScript);
        }
        return scripts;
    }

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