using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A payroll wage type
/// </summary>
public class WageType : ScriptTrackDomainObject<WageTypeAudit>, IDerivableObject, IClusterObject,
    INamedObject, INamespaceObject, IDomainAttributeObject, IEquatable<WageType>
{
    private static readonly List<FunctionType> FunctionTypes =
    [
        FunctionType.WageTypeValue,
        FunctionType.WageTypeResult
    ];

    /// <summary>
    /// The wage type number (immutable)
    /// </summary>
    public decimal WageTypeNumber { get; set; }

    /// <summary>
    /// The wage type name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The localized wage type names
    /// </summary>
    public Dictionary<string, string> NameLocalizations { get; set; }

    /// <summary>
    /// The wage type description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The localized wage type descriptions
    /// </summary>
    public Dictionary<string, string> DescriptionLocalizations { get; set; }

    /// <summary>
    /// The override type
    /// </summary>
    public OverrideType OverrideType { get; set; }

    /// <summary>
    /// The value type, default is value type money
    /// </summary>
    public ValueType ValueType
    {
        get;
        set
        {
            if (!value.IsNumber())
            {
                throw new ArgumentOutOfRangeException(nameof(value),
                    $"Value type of wage type must be a number: {value}");
            }

            field = value;
        }
    } = ValueType.Money;

    /// <summary>
    /// The wage type calendar (fallback: employee calendar)
    /// </summary>
    public string Calendar { get; set; }

    /// <summary>
    /// The wage type culture name based on RFC 4646
    /// </summary>
    public string Culture { get; set; }

    /// <summary>
    /// Associated collectors
    /// </summary>
    public List<string> Collectors { get; set; }

    /// <summary>
    /// Associated collector groups
    /// </summary>
    public List<string> CollectorGroups { get; set; }

    /// <summary>
    /// Expression: calculates of the wage type value
    /// </summary>
    public string ValueExpression { get; set; }

    /// <summary>
    /// Expression: calculates of the wage type result attributes
    /// </summary>
    public string ResultExpression { get; set; }

    /// <summary>
    /// The wage type value actions
    /// </summary>
    public List<string> ValueActions { get; set; }

    /// <summary>
    /// The wage type result actions
    /// </summary>
    public List<string> ResultActions { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <summary>
    /// The wage type clusters
    /// </summary>
    public List<string> Clusters { get; set; }

    /// <inheritdoc/>
    public WageType()
    {
    }

    /// <inheritdoc/>
    protected WageType(WageType copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <inheritdoc/>
    public void ApplyNamespace(string @namespace)
    {
        Name = Name.EnsureNamespace(@namespace);
        Collectors = Collectors.EnsureNamespace(@namespace);
        CollectorGroups = CollectorGroups.EnsureNamespace(@namespace);
        Clusters = Clusters.EnsureNamespace(@namespace);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(WageType compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override WageTypeAudit ToAuditObject()
    {
        if (!this.HasId())
        {
            return null;
        }
        return new()
        {
            WageTypeId = Id,
            WageTypeNumber = WageTypeNumber,
            Name = Name,
            Description = Description,
            OverrideType = OverrideType,
            ValueType = ValueType,
            Calendar = Calendar,
            Culture = Culture,
            Collectors = Collectors,
            CollectorGroups = CollectorGroups,
            ValueExpression = ValueExpression,
            ResultExpression = ResultExpression,
            ValueActions = ValueActions,
            ResultActions = ResultActions,
            Script = Script,
            ScriptVersion = ScriptVersion,
            Binary = Binary,
            ScriptHash = ScriptHash,
            Attributes = Attributes,
            Clusters = Clusters
        };
    }

    /// <inheritdoc/>
    public override void FromAuditObject(WageTypeAudit audit)
    {
        base.FromAuditObject(audit);

        Id = audit.WageTypeId;
        WageTypeNumber = audit.WageTypeNumber;
        Name = audit.Name;
        Description = audit.Description;
        OverrideType = audit.OverrideType;
        ValueType = audit.ValueType;
        Calendar = audit.Calendar;
        Culture = audit.Culture;
        Collectors = audit.Collectors;
        CollectorGroups = audit.CollectorGroups;
        ValueExpression = audit.ValueExpression;
        ResultExpression = audit.ResultExpression;
        ValueActions = audit.ValueActions;
        ResultActions = audit.ResultActions;
        Attributes = audit.Attributes;
        Clusters = audit.Clusters;
    }

    #region Scripting

    /// <summary>
    /// Test for value script
    /// </summary>
    public string ValueScript =>
        HasValueScript ? ValueExpression ?? "0" : null;

    /// <summary>
    /// Test for result script
    /// </summary>
    public string ResultScript =>
        HasResultScript ? ResultExpression ?? "0" : null;

    /// <summary>
    /// Test for value script
    /// </summary>
    private bool HasValueScript =>
        AnyExpressionOrActions(ValueExpression, ValueActions);

    /// <summary>
    /// Test for result script
    /// </summary>
    private bool HasResultScript =>
        AnyExpressionOrActions(ResultExpression, ResultActions);

    /// <inheritdoc/>
    public override bool HasAnyExpression =>
        HasValueScript ||
        HasResultScript;

    /// <inheritdoc/>
    public override bool HasAnyAction =>
        AnyActions(ValueActions) ||
        AnyActions(ResultActions);

    /// <inheritdoc/>
    public override bool HasObjectScripts => true;

    /// <inheritdoc/>
    public override List<FunctionType> GetFunctionTypes() => FunctionTypes;

    /// <inheritdoc/>
    public override string GetFunctionScript(FunctionType functionType) =>
        functionType switch
        {
            FunctionType.WageTypeValue => ValueExpression,
            FunctionType.WageTypeResult => ResultExpression,
            _ => null
        };

    /// <inheritdoc/>
    public override List<string> GetFunctionActions(FunctionType functionType) =>
        functionType switch
        {
            FunctionType.WageTypeValue => ValueActions,
            FunctionType.WageTypeResult => ResultActions,
            _ => null
        };

    /// <inheritdoc/>
    public override IEnumerable<string> GetEmbeddedScriptNames() =>
        GetEmbeddedScriptNames([
            new(ValueExpression, ValueActions, FunctionType.WageTypeValue),
            new(ResultExpression, ResultActions, FunctionType.WageTypeResult)
        ]);

    #endregion

    /// <inheritdoc/>
    public override string ToString() =>
        $"{WageTypeNumber:##.####} {Name} {base.ToString()}";
}