using System;
using System.Linq;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A payroll collector
/// </summary>
public class Collector : ScriptTrackDomainObject<CollectorAudit>, IDerivableObject, IClusterObject,
    INamedObject, INamespaceObject, IDomainAttributeObject, IEquatable<Collector>
{
    private static readonly List<FunctionType> FunctionTypes =
    [
        FunctionType.CollectorStart,
        FunctionType.CollectorApply,
        FunctionType.CollectorEnd
    ];

    // collected values
    private readonly List<decimal> values = [];

    /// <summary>
    /// The collector name (immutable)
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The localized collector names
    /// </summary>
    public Dictionary<string, string> NameLocalizations { get; set; }

    /// <summary>
    /// The collect mode (immutable)
    /// </summary>
    public CollectMode CollectMode { get; set; }

    /// <summary>
    /// Negated collector result (immutable)
    /// </summary>
    public bool Negated { get; set; }

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
                    $"Value type of collector must be a number: {value}");
            }

            field = value;
        }
    } = ValueType.Money;

    /// <summary>
    /// The collector culture name based on RFC 4646
    /// </summary>
    public string Culture { get; set; }

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
    /// The collector start actions
    /// </summary>
    public List<string> StartActions { get; set; }

    /// <summary>
    /// The collector apply actions
    /// </summary>
    public List<string> ApplyActions { get; set; }

    /// <summary>
    /// The collector end actions
    /// </summary>
    public List<string> EndActions { get; set; }

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
        Threshold.HasValue ? Math.Max(Result, Threshold.Value) : 0;

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
            var value = CollectMode switch
            {
                CollectMode.Summary => Summary,
                CollectMode.Minimum => Minimum,
                CollectMode.Maximum => Maximum,
                CollectMode.Average => Average,
                CollectMode.Range => Range,
                CollectMode.Count => values.Count,
                _ => throw new ArgumentOutOfRangeException()
            };

            // negated
            if (Negated)
            {
                value = decimal.Negate(value);
            }

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

    /// <inheritdoc/>
    public void ApplyNamespace(string @namespace)
    {
        Name = Name.EnsureNamespace(@namespace);
        CollectorGroups = CollectorGroups.EnsureNamespace(@namespace);
        Clusters = Clusters.EnsureNamespace(@namespace);
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
            CollectMode = CollectMode,
            Negated = Negated,
            OverrideType = OverrideType,
            ValueType = ValueType,
            Culture = Culture,
            CollectorGroups = CollectorGroups,
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
            StartActions = StartActions,
            ApplyActions = ApplyActions,
            EndActions = EndActions,
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
        CollectMode = audit.CollectMode;
        Negated = audit.Negated;
        OverrideType = audit.OverrideType;
        ValueType = audit.ValueType;
        Culture = audit.Culture;
        CollectorGroups = audit.CollectorGroups;
        Threshold = audit.Threshold;
        MinResult = audit.MinResult;
        MaxResult = audit.MaxResult;
        StartExpression = audit.StartExpression;
        ApplyExpression = audit.ApplyExpression;
        EndExpression = audit.EndExpression;
        StartActions = audit.StartActions;
        ApplyActions = audit.ApplyActions;
        EndActions = audit.EndActions;
        Attributes = audit.Attributes;
        Clusters = audit.Clusters;
    }

    #region Scripting

    /// <summary>
    /// Test for start script
    /// </summary>
    public string StartScript =>
        HasStartScript ? StartExpression ?? "0" : null;

    /// <summary>
    /// Test for apply script
    /// </summary>
    public string ApplyScript =>
        HasApplyScript ? ApplyExpression ?? "0" : null;

    /// <summary>
    /// Test for end script
    /// </summary>
    public string EndScript =>
        HasEndScript ? EndExpression ?? "0" : null;

    /// <summary>
    /// Test for start script
    /// </summary>
    private bool HasStartScript =>
        AnyExpressionOrActions(StartExpression, StartActions);

    /// <summary>
    /// Test for apply script
    /// </summary>
    private bool HasApplyScript =>
        AnyExpressionOrActions(ApplyExpression, ApplyActions);

    /// <summary>
    /// Test for end script
    /// </summary>
    private bool HasEndScript =>
        AnyExpressionOrActions(EndExpression, EndActions);

    /// <inheritdoc/>
    public override bool HasAnyExpression =>
        HasStartScript ||
        HasApplyScript ||
        HasEndScript;

    /// <inheritdoc/>
    public override bool HasAnyAction =>
        AnyActions(StartActions) ||
        AnyActions(ApplyActions) ||
        AnyActions(EndActions);

    /// <inheritdoc/>
    public override bool HasObjectScripts => true;

    /// <inheritdoc/>
    public override List<FunctionType> GetFunctionTypes() => FunctionTypes;

    /// <inheritdoc/>
    public override string GetFunctionScript(FunctionType functionType) =>
        functionType switch
        {
            FunctionType.CollectorStart => StartExpression,
            FunctionType.CollectorApply => ApplyExpression,
            FunctionType.CollectorEnd => EndExpression,
            _ => null
        };

    /// <inheritdoc/>
    public override List<string> GetFunctionActions(FunctionType functionType) =>
        functionType switch
        {
            FunctionType.CollectorStart => StartActions,
            FunctionType.CollectorApply => ApplyActions,
            FunctionType.CollectorEnd => EndActions,
            _ => null
        };

    /// <inheritdoc/>
    public override IEnumerable<string> GetEmbeddedScriptNames() =>
        GetEmbeddedScriptNames([
            new(StartExpression, StartActions, FunctionType.CollectorStart),
            new(ApplyExpression, ApplyActions, FunctionType.CollectorApply),
            new(EndExpression, EndActions, FunctionType.CollectorEnd)
        ]);

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
        var negated = Negated ? " -" : string.Empty;
        return $"{Name}={Result} ({CollectMode}{negated}) {base.ToString()}";
    }
}