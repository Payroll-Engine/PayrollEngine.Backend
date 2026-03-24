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
        /// <summary>Wage type value of the previous period</summary>
        PrevPeriod,
        /// <summary>Wage type value of the next period</summary>
        NextPeriod,
        /// <summary>Wage type cycle value, the year-to-date value</summary>
        Cycle,
        /// <summary>Wage type total value of the previous cycle</summary>
        PrevCycle,
        /// <summary>Wage type total value of the next cycle</summary>
        NextCycle,
        /// <summary>Sum of retro corrections for the wage type within the current cycle</summary>
        RetroSum
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
            ValueScope.PrevPeriod => ValueScope.PrevPeriod,
            ValueScope.NextPeriod => ValueScope.NextPeriod,
            ValueScope.Cycle => ValueScope.Cycle,
            ValueScope.PrevCycle => ValueScope.PrevCycle,
            ValueScope.NextCycle => ValueScope.NextCycle,
            ValueScope.RetroSum => ValueScope.RetroSum,
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
                    // wage type value by number - numeric references never use namespace prefix
                    return new(parseData, $"{nameof(WageTypeFunction.GetWageTypeValueByNumber)}({parseData.Text})");
                }
                // wage type value by name
                return new(parseData, $"{nameof(WageTypeFunction.GetWageTypeValueByName)}(\"{wageTypeName}\")");

            case ValueScope.PrevPeriod:
                if (decimal.TryParse(parseData.Text,
                        style: NumberStyles.Any,
                        provider: CultureInfo.InvariantCulture,
                        result: out _))
                {
                    return new(parseData, $"{nameof(PayrunFunction.GetPrevPeriodWageTypeValue)}({parseData.Text})");
                }
                return new(parseData, $"{nameof(PayrunFunction.GetPrevPeriodWageTypeValue)}(\"{wageTypeName}\")");

            case ValueScope.NextPeriod:
                if (decimal.TryParse(parseData.Text,
                        style: NumberStyles.Any,
                        provider: CultureInfo.InvariantCulture,
                        result: out _))
                {
                    return new(parseData, $"{nameof(PayrunFunction.GetNextPeriodWageTypeValue)}({parseData.Text})");
                }
                return new(parseData, $"{nameof(PayrunFunction.GetNextPeriodWageTypeValue)}(\"{wageTypeName}\")");

            case ValueScope.Cycle:
                // wage type cycle value — always by name (numeric resolved via GetWageTypeNumber in action method)
                return new(parseData, $"{nameof(PayrunFunction.GetCycleWageTypeValue)}(\"{wageTypeName}\")");

            case ValueScope.PrevCycle:
                if (decimal.TryParse(parseData.Text,
                        style: NumberStyles.Any,
                        provider: CultureInfo.InvariantCulture,
                        result: out _))
                {
                    return new(parseData, $"{nameof(PayrunFunction.GetPrevCycleWageTypeValue)}({parseData.Text})");
                }
                return new(parseData, $"{nameof(PayrunFunction.GetPrevCycleWageTypeValue)}(\"{wageTypeName}\")");

            case ValueScope.NextCycle:
                if (decimal.TryParse(parseData.Text,
                        style: NumberStyles.Any,
                        provider: CultureInfo.InvariantCulture,
                        result: out _))
                {
                    return new(parseData, $"{nameof(PayrunFunction.GetNextCycleWageTypeValue)}({parseData.Text})");
                }
                return new(parseData, $"{nameof(PayrunFunction.GetNextCycleWageTypeValue)}(\"{wageTypeName}\")");

            case ValueScope.RetroSum:
                // sum of retro corrections for the wage type within the current cycle
                return new(parseData, $"{nameof(WageTypeFunction.GetRetroWageTypeValueSum)}(\"{wageTypeName}\")");

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
