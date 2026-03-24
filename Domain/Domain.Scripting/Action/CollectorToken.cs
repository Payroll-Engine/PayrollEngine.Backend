using System;
using PayrollEngine.Action;
using PayrollEngine.Client.Scripting.Function;

namespace PayrollEngine.Domain.Scripting.Action;

/// <summary>
/// Action token for collector
/// </summary>
internal sealed class CollectorToken : TokenBase
{
    /// <summary>Collector value scope</summary>
    private enum ValueScope
    {
        /// <summary>Collector period value (default)</summary>
        Period,
        /// <summary>Collector value of the previous period</summary>
        PrevPeriod,
        /// <summary>Collector value of the next period</summary>
        NextPeriod,
        /// <summary>Collector cycle value, the year-to-date value</summary>
        Cycle,
        /// <summary>Collector total value of the previous cycle</summary>
        PrevCycle,
        /// <summary>Collector total value of the next cycle</summary>
        NextCycle
    }

    /// <summary>Default constructor</summary>
    /// <param name="action">Action text</param>
    /// <param name="startIndex">Token start index</param>
    /// <param name="namespace">The token namespace</param>
    internal CollectorToken(string action, int startIndex, string @namespace = null) :
        base(TokenType.ReadProperty, action, startIndex, @namespace)
    {
    }

    /// <inheritdoc />
    internal override bool SupportedFunction(FunctionType functionType) =>
        MarkerType.Collector.SupportedFunction(functionType);

    /// <inheritdoc />
    protected override TokenResultData EvaluateToken(TokenParseData parseData)
    {
        if (string.IsNullOrWhiteSpace(parseData.Text))
        {
            return null;
        }

        // namespace
        var collectorName = parseData.Text.EnsureNamespace(Namespace);

        // token property
        var scope = parseData.GetProperty<ValueScope>() switch
        {
            ValueScope.Period => ValueScope.Period,
            ValueScope.PrevPeriod => ValueScope.PrevPeriod,
            ValueScope.NextPeriod => ValueScope.NextPeriod,
            ValueScope.Cycle => ValueScope.Cycle,
            ValueScope.PrevCycle => ValueScope.PrevCycle,
            ValueScope.NextCycle => ValueScope.NextCycle,
            _ => ValueScope.Period
        };

        // code
        return scope switch
        {
            ValueScope.Period =>
                new(parseData, $"{nameof(WageTypeFunction.GetCollectorValue)}(\"{collectorName}\")"),
            ValueScope.PrevPeriod =>
                new(parseData, $"{nameof(PayrunFunction.GetPrevPeriodCollectorValue)}(\"{collectorName}\")"),
            ValueScope.NextPeriod =>
                new(parseData, $"{nameof(PayrunFunction.GetNextPeriodCollectorValue)}(\"{collectorName}\")"),
            ValueScope.Cycle =>
                new(parseData, $"{nameof(PayrunFunction.GetCycleCollectorValue)}(\"{collectorName}\")"),
            ValueScope.PrevCycle =>
                new(parseData, $"{nameof(PayrunFunction.GetPrevCycleCollectorValue)}(\"{collectorName}\")"),
            ValueScope.NextCycle =>
                new(parseData, $"{nameof(PayrunFunction.GetNextCycleCollectorValue)}(\"{collectorName}\")"),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
