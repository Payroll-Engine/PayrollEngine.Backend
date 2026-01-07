using System;
using System.Globalization;
using PayrollEngine.Action;
using PayrollEngine.Client.Scripting.Function;

namespace PayrollEngine.Domain.Scripting.Action;

/// <summary>
/// Action token for wage type
/// </summary>
internal sealed class WageTypeToken : TokenBase
{
    /// <summary>Wage type value scope</summary>
    private enum ValueScope
    {
        /// <summary>Wage type period value (default)</summary>
        Period,
        /// <summary>Wage type cycle value, the year-to-date value</summary>
        Cycle
    }

    /// <summary>Default constructor</summary>
    /// <param name="action">Action text</param>
    /// <param name="startIndex">Token start index</param>
    /// <param name="namespace">The token namespace</param>
    internal WageTypeToken(string action, int startIndex, string @namespace = null) :
        base(TokenType.ReadProperty, action, startIndex, @namespace)
    {
    }

    /// <inheritdoc />
    internal override bool SupportedFunction(FunctionType functionType) =>
        MarkerType.WageType.SupportedFunction(functionType);

    /// <inheritdoc />
    protected override TokenResultData EvaluateToken(TokenParseData parseData)
    {
        if (string.IsNullOrWhiteSpace(parseData.Text))
        {
            return null;
        }

        // namespace
        var wageTypeName = parseData.Text.EnsureNamespace(Namespace);

        // token property
        var scope = parseData.GetProperty<ValueScope>() switch
        {
            ValueScope.Period => ValueScope.Period,
            ValueScope.Cycle => ValueScope.Cycle,
            _ => ValueScope.Period
        };

        // code
        switch (scope)
        {
            case ValueScope.Period:
                if (decimal.TryParse(parseData.Text,
                        style: NumberStyles.Any,
                        provider: CultureInfo.InvariantCulture,
                        result: out _))
                {
                    // wage type value by number
                    return new(parseData, $"{nameof(WageTypeFunction.GetWageTypeValueByNumber)}({wageTypeName})");
                }
                // wage type value by name
                return new(parseData, $"{nameof(WageTypeFunction.GetWageTypeValueByName)}(\"{wageTypeName}\")");
            case ValueScope.Cycle:
                // wage type cycle value
                return new(parseData, $"{nameof(WageTypeFunction.GetCycleWageTypeValue)}(\"{wageTypeName}\")");
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}