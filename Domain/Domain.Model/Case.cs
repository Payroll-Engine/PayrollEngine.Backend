using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A national, company and employee case
/// </summary>
public class Case : ScriptTrackDomainObject<CaseAudit>, IDerivableObject, IClusterObject,
    INamedObject, INamespaceObject, IDomainAttributeObject, IEquatable<Case>
{
    private static readonly List<FunctionType> FunctionTypes =
    [
        FunctionType.CaseAvailable,
        FunctionType.CaseBuild,
        FunctionType.CaseValidate
    ];

    /// <summary>
    /// The case name (immutable)
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The localized case names
    /// </summary>
    public Dictionary<string, string> NameLocalizations { get; set; }

    /// <summary>
    /// Synonyms for the case name
    /// </summary>
    public List<string> NameSynonyms { get; set; }

    /// <summary>
    /// The case description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The localized case descriptions
    /// </summary>
    public Dictionary<string, string> DescriptionLocalizations { get; set; }

    /// <summary>
    /// The default case change reason
    /// </summary>
    public string DefaultReason { get; set; }

    /// <summary>
    /// The localized default case change reasons
    /// </summary>
    public Dictionary<string, string> DefaultReasonLocalizations { get; set; }

    /// <summary>
    /// The type of he case (immutable)
    /// </summary>
    public CaseType CaseType { get; set; }

    /// <summary>
    /// The base case name
    /// </summary>
    public string BaseCase { get; set; }

    /// <summary>
    /// The base case fields
    /// </summary>
    public List<CaseFieldReference> BaseCaseFields { get; set; }

    /// <summary>
    /// The override type
    /// </summary>
    public OverrideType OverrideType { get; set; }

    /// <summary>
    /// The cancellation type
    /// </summary>
    public CaseCancellationType CancellationType { get; set; }

    /// <summary>
    /// Hidden case (default: false)
    /// </summary>
    public bool Hidden { get; set; }

    /// <summary>
    /// The expression used to build a case
    /// </summary>
    public string AvailableExpression { get; set; }

    /// <summary>
    /// The expression used to build the case
    /// </summary>
    public string BuildExpression { get; set; }

    /// <summary>
    /// The expression which evaluates if the case is valid
    /// </summary>
    public string ValidateExpression { get; set; }

    /// <summary>
    /// The case lookups
    /// </summary>
    public List<string> Lookups { get; set; }

    /// <summary>
    /// The case slots
    /// </summary>
    public List<CaseSlot> Slots { get; set; }

    /// <summary>
    /// The case available actions
    /// </summary>
    public List<string> AvailableActions { get; set; }

    /// <summary>
    /// The case build actions
    /// </summary>
    public List<string> BuildActions { get; set; }

    /// <summary>
    /// The case validate actions
    /// </summary>
    public List<string> ValidateActions { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <summary>
    /// The case clusters
    /// </summary>
    public List<string> Clusters { get; set; }

    /// <inheritdoc/>
    public Case()
    {
    }

    /// <inheritdoc/>
    protected Case(Case copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <inheritdoc/>
    public void ApplyNamespace(string @namespace)
    {
        Name = Name.EnsureNamespace(@namespace);
        BaseCase = BaseCase.EnsureNamespace(@namespace);
        if (BaseCaseFields != null)
        {
            var baseCaseFields = new List<CaseFieldReference>();
            foreach (var baseCaseField in BaseCaseFields)
            {
                baseCaseFields.Add(new CaseFieldReference
                {
                    Name = baseCaseField.Name.EnsureNamespace(@namespace),
                    Order = baseCaseField.Order
                });
            }
            BaseCaseFields = baseCaseFields;
        }
        Clusters = Clusters.EnsureNamespace(@namespace);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(Case compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override CaseAudit ToAuditObject()
    {
        if (!this.HasId())
        {
            return null;
        }
        return new()
        {
            CaseId = Id,
            CaseType = CaseType,
            Name = Name,
            NameLocalizations = NameLocalizations,
            NameSynonyms = NameSynonyms,
            Description = Description,
            DescriptionLocalizations = DescriptionLocalizations,
            DefaultReason = DefaultReason,
            DefaultReasonLocalizations = DefaultReasonLocalizations,
            BaseCase = BaseCase,
            BaseCaseFields = BaseCaseFields,
            OverrideType = OverrideType,
            CancellationType = CancellationType,
            Hidden = Hidden,
            AvailableExpression = AvailableExpression,
            BuildExpression = BuildExpression,
            ValidateExpression = ValidateExpression,
            Lookups = Lookups,
            Slots = Slots,
            Script = Script,
            ScriptVersion = ScriptVersion,
            Binary = Binary,
            ScriptHash = ScriptHash,
            AvailableActions = AvailableActions,
            BuildActions = BuildActions,
            ValidateActions = ValidateActions,
            Attributes = Attributes,
            Clusters = Clusters
        };
    }

    /// <inheritdoc/>
    public override void FromAuditObject(CaseAudit audit)
    {
        // base values
        base.FromAuditObject(audit);

        // local values
        // keep in sync with ctor
        Id = audit.CaseId;
        CaseType = audit.CaseType;
        Name = audit.Name;
        NameLocalizations = audit.NameLocalizations;
        NameSynonyms = audit.NameSynonyms;
        Description = audit.Description;
        DescriptionLocalizations = audit.DescriptionLocalizations;
        DefaultReason = audit.DefaultReason;
        DefaultReasonLocalizations = audit.DefaultReasonLocalizations;
        BaseCase = audit.BaseCase;
        BaseCaseFields = audit.BaseCaseFields;
        OverrideType = audit.OverrideType;
        CancellationType = audit.CancellationType;
        Hidden = audit.Hidden;
        AvailableExpression = audit.AvailableExpression;
        BuildExpression = audit.BuildExpression;
        ValidateExpression = audit.ValidateExpression;
        Lookups = audit.Lookups;
        Slots = audit.Slots;
        AvailableActions = audit.AvailableActions;
        BuildActions = audit.BuildActions;
        ValidateActions = audit.ValidateActions;
        Attributes = audit.Attributes;
        Clusters = audit.Clusters;
    }

    #region Scripting

    /// <summary>
    /// Test for available script
    /// </summary>
    public string AvailableScript =>
        HasAvailableScript ? AvailableExpression ?? "true" : null;

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
    /// Test for available script
    /// </summary>
    private bool HasAvailableScript =>
        AnyExpressionOrActions(AvailableExpression, AvailableActions);

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
        HasAvailableScript ||
        HasBuildScript ||
        HasValidateScript;

    /// <inheritdoc/>
    public override bool HasAnyAction =>
        AnyActions(AvailableActions) ||
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
            FunctionType.CaseAvailable => AvailableExpression,
            FunctionType.CaseBuild => BuildExpression,
            FunctionType.CaseValidate => ValidateExpression,
            _ => null
        };

    /// <inheritdoc/>
    public override List<string> GetFunctionActions(FunctionType functionType) =>
        functionType switch
        {
            FunctionType.CaseAvailable => AvailableActions,
            FunctionType.CaseBuild => BuildActions,
            FunctionType.CaseValidate => ValidateActions,
            _ => null
        };

    /// <inheritdoc/>
    public override IEnumerable<string> GetEmbeddedScriptNames() =>
        GetEmbeddedScriptNames([
            new(AvailableExpression, AvailableActions, FunctionType.CaseAvailable),
            new(BuildExpression, BuildActions, FunctionType.CaseBuild),
            new(ValidateExpression, ValidateActions, FunctionType.CaseValidate)
        ]);

    #endregion

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Name} {base.ToString()}";
}