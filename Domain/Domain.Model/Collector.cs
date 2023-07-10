using System;
using System.Collections.Generic;
using System.Linq;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A payroll collector
/// </summary>
public class Collector : ScriptTrackDomainObject<CollectorAudit>, IDerivableObject, IClusterObject,
    INamedObject, IDomainAttributeObject, IEquatable<Collector>
{
    private static readonly List<FunctionType> FunctionTypes = new()
    {
        FunctionType.CollectorStart,
        FunctionType.CollectorApply,
        FunctionType.CollectorEnd
    };
    private static readonly IEnumerable<string> EmbeddedScriptNames = new[]
    {
        "Cache\\Cache.cs",
        "Function\\PayrunFunction.cs",
        "Function\\CollectorFunction.cs"
    };

    // collected values
    private readonly List<decimal> values = new();

    /// <summary>
    /// The collector name (immutable)
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The localized collector names
    /// </summary>
    public Dictionary<string, string> NameLocalizations { get; set; }

    /// <summary>
    /// The collection type (immutable)
    /// </summary>
    public CollectType CollectType { get; set; }

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
                    $"Value type of collector must be a number: {value}");
            }
            valueType = value;
        }
    }

    /// <summary>
    /// Associated collector groups
    /// </summary>
    public List<string> CollectorGroups { get; set; }

    /// <summary>
    /// The threshold value
    /// </summary>
    public decimal? Threshold { get; set; }

    /// <summary>
    /// The minimum allowed value
    /// </summary>
    public decimal? MinResult { get; set; }

    /// <summary>
    /// The maximum allowed value
    /// </summary>
    public decimal? MaxResult { get; set; }

    /// <summary>
    /// Expression used while the collector is started
    /// </summary>
    public string StartExpression { get; set; }

    /// <summary>
    /// Expression used while applying a value to the collector
    /// </summary>
    public string ApplyExpression { get; set; }

    /// <summary>
    /// Expression used while the collector is ended
    /// </summary>
    public string EndExpression { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <summary>
    /// The collector clusters
    /// </summary>
    public List<string> Clusters { get; set; }

    #region Calculations

    /// <summary>
    /// The minimum value allowed
    /// </summary>
    public static readonly decimal MinValue = decimal.MinValue;

    /// <summary>
    /// The maximum value allowed
    /// </summary>
    public static readonly decimal MaxValue = decimal.MaxValue;

    /// <summary>
    /// The collected result value restricted against the threshold value
    /// </summary>
    public decimal? ResultThreshold =>
        Threshold.HasValue ? Math.Max(Result, Threshold.Value) : default;

    /// <summary>
    /// The surplus of the collected result value against the threshold value
    /// </summary>
    public decimal? ResultThresholdSurplus
    {
        get
        {
            if (Threshold.HasValue)
            {
                return null;
            }
            var thresholdResult = Result;
            return thresholdResult > Threshold ? thresholdResult - Threshold : 0M;
        }
    }

    /// <summary>
    /// Collected values count
    /// </summary>
    public decimal Count => values.Count;

    /// <summary>
    /// The summary of the collected values
    /// </summary>
    public decimal Summary => values.Sum();

    /// <summary>
    /// The minimum collected values
    /// </summary>
    public decimal Minimum => values.Any() ? values.Max() : 0M;

    /// <summary>
    /// The maximum collected value
    /// </summary>
    public decimal Maximum => values.Any() ? values.Max() : 0M;

    /// <summary>
    /// The average of the collected values
    /// </summary>
    public decimal Average => values.Any() ? values.Average() : 0M;

    /// <summary>
    /// The range of the collected values
    /// </summary>
    public decimal Range => Maximum - Minimum;

    /// <summary>
    /// The collected result value
    /// </summary>
    public decimal Result
    {
        get
        {
            // collect
            var value = CollectType switch
            {
                CollectType.Summary => Summary,
                CollectType.Minimum => Minimum,
                CollectType.Maximum => Maximum,
                CollectType.Average => Average,
                CollectType.Range => Range,
                CollectType.Count => values.Count,
                _ => throw new ArgumentOutOfRangeException()
            };

            // min range
            if (value < MinResult)
            {
                return MinResult.Value;
            }

            // max range
            if (value > MaxResult)
            {
                return MaxResult.Value;
            }

            // result
            return value;
        }
    }

    #endregion

    /// <summary>
    /// Gets the collector values
    /// </summary>
    /// <returns>The collector values</returns>
    public IEnumerable<decimal> GetValues() => values;

    /// <summary>
    /// Sets the collector values
    /// </summary>
    /// <returns>The collector values</returns>
    public void SetValues(IEnumerable<decimal> newValues)
    {
        Reset();
        values.AddRange(newValues);
    }

    /// <summary>
    /// Add a new collector value
    /// </summary>
    public void AddValue(decimal value)
    {
        values.Add(value);
    }

    /// <summary>
    /// Resets the collector to his initial state
    /// </summary>
    public void Reset()
    {
        values.Clear();
    }

    /// <inheritdoc/>
    public Collector()
    {
    }

    /// <inheritdoc/>
    protected Collector(Collector copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(Collector compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override CollectorAudit ToAuditObject()
    {
        if (!this.HasId())
        {
            return null;
        }

        return new()
        {
            CollectorId = Id,
            Name = Name,
            NameLocalizations = NameLocalizations,
            CollectType = CollectType,
            OverrideType = OverrideType,
            CollectorGroups = CollectorGroups,
            ValueType = ValueType,
            Threshold = Threshold,
            MinResult = MinResult,
            MaxResult = MaxResult,
            Script = Script,
            ScriptVersion = ScriptVersion,
            Binary = Binary,
            ScriptHash = ScriptHash,
            StartExpression = StartExpression,
            ApplyExpression = ApplyExpression,
            EndExpression = EndExpression,
            Attributes = Attributes,
            Clusters = Clusters
        };
    }

    /// <inheritdoc/>
    public override void FromAuditObject(CollectorAudit audit)
    {
        base.FromAuditObject(audit);

        Id = audit.CollectorId;
        Name = audit.Name;
        NameLocalizations = audit.NameLocalizations;
        CollectType = audit.CollectType;
        OverrideType = audit.OverrideType;
        CollectorGroups = audit.CollectorGroups;
        ValueType = audit.ValueType;
        Threshold = audit.Threshold;
        MinResult = audit.MinResult;
        MaxResult = audit.MaxResult;
        StartExpression = audit.StartExpression;
        ApplyExpression = audit.ApplyExpression;
        EndExpression = audit.EndExpression;
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
        if (!string.IsNullOrWhiteSpace(StartExpression))
        {
            scripts.Add(FunctionType.CollectorStart, StartExpression);
        }
        if (!string.IsNullOrWhiteSpace(ApplyExpression))
        {
            scripts.Add(FunctionType.CollectorApply, ApplyExpression);
        }
        if (!string.IsNullOrWhiteSpace(EndExpression))
        {
            scripts.Add(FunctionType.CollectorEnd, EndExpression);
        }
        return scripts;
    }

    /// <inheritdoc/>
    public override IEnumerable<string> GetEmbeddedScriptNames() =>
        EmbeddedScriptNames;

    #endregion

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Name}={Result} ({CollectType}) {base.ToString()}";
}