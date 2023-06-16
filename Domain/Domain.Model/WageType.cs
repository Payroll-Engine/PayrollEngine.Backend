using System;
using System.Collections.Generic;
using System.Linq;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A payroll wage type
/// </summary>
public class WageType : ScriptTrackDomainObject<WageTypeAudit>, IDerivableObject, IClusterObject,
    INamedObject, IDomainAttributeObject, IEquatable<WageType>
{
    private static readonly List<FunctionType> FunctionTypes = new()
    {
        FunctionType.WageTypeValue,
        FunctionType.WageTypeResult
    };
    private static readonly IEnumerable<string> EmbeddedScriptNames = new[]
    {
        "Cache\\Cache.cs",
        "Function\\PayrunFunction.cs",
        "Function\\WageTypeFunction.cs"
    };

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

    private ValueType valueType = ValueType.Money;
    /// <summary>
    /// The value type, default is value type money
    /// </summary>
    public ValueType ValueType
    {
        get => valueType;
        set
        {
            if (!value.IsNumber())
            {
                throw new ArgumentOutOfRangeException(nameof(value),
                    $"Value type of wage type must be a number: {value}");
            }
            valueType = value;
        }
    }

    /// <summary>
    /// The wage type calendar
    /// </summary>
    public string Calendar { get; set; }

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
    public WageType(WageType copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
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
            Collectors = Collectors,
            CollectorGroups = CollectorGroups,
            ValueExpression = ValueExpression,
            ResultExpression = ResultExpression,
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
        Collectors = audit.Collectors;
        CollectorGroups = audit.CollectorGroups;
        ValueExpression = audit.ValueExpression;
        ResultExpression = audit.ResultExpression;
        Attributes = audit.Attributes;
        Clusters = audit.Clusters;
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
        if (!string.IsNullOrWhiteSpace(ValueExpression))
        {
            scripts.Add(FunctionType.WageTypeValue, ValueExpression);
        }
        if (!string.IsNullOrWhiteSpace(ResultExpression))
        {
            scripts.Add(FunctionType.WageTypeResult, ResultExpression);
        }
        return scripts;
    }

    /// <inheritdoc/>
    public override IEnumerable<string> GetEmbeddedScriptNames() =>
        EmbeddedScriptNames;

    #endregion

    /// <inheritdoc/>
    public override string ToString() =>
        $"{WageTypeNumber:##.####} {Name} {base.ToString()}";
}