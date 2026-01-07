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
        /// <summary>Collector cycle value, the year-to-date value</summary>
        Cycle
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
            ValueScope.Cycle => ValueScope.Cycle,
            _ => ValueScope.Period
        };

        // code
        return scope switch
        {
            ValueScope.Period => new(parseData, $"{nameof(WageTypeFunction.GetCollectorValue)}(\"{collectorName}\")"),
            ValueScope.Cycle => new(parseData, $"{nameof(WageTypeFunction.GetCycleCollectorValue)}(\"{collectorName}\")"),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}