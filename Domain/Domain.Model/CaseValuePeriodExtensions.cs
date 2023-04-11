using System;
using System.Collections.Generic;
using System.Linq;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Extension methods for the <see cref="CaseValue"/>
/// </summary>
public static class CaseValuePeriodExtensions
{
    /// <summary>
    /// Get summary of case values
    /// </summary>
    /// <param name="caseValues">The case values</param>
    /// <returns>Case value with the summary of the case values</returns>
    public static string Summary(this IEnumerable<CaseValue> caseValues)
    {
        if (caseValues == null)
        {
            throw new ArgumentNullException(nameof(caseValues));
        }

        // case values
        var listCaseValues = caseValues.ToList();
        if (!listCaseValues.Any())
        {
            throw new ArgumentException(nameof(caseValues));
        }

        // reference case value
        var firstCaseValue = listCaseValues.First();
        if (!firstCaseValue.ValueType.IsNumber())
        {
            throw new InvalidOperationException($"Invalid value type {firstCaseValue.ValueType} for case value summary");
        }

        // build values summary
        var valueType = firstCaseValue.ValueType;
        var intSummary = 0;
        decimal decimalSummary = 0;
        foreach (var caseValue in listCaseValues)
        {
            if (caseValue.ValueType != valueType)
            {
                throw new InvalidOperationException($"Mismatching value type between {firstCaseValue}={valueType} and {caseValue}={caseValue.ValueType}");
            }
            if (string.IsNullOrWhiteSpace(caseValue.Value))
            {
                continue;
            }

            // integer
            if (valueType == ValueType.Integer)
            {
                intSummary += caseValue.Value.JsonToInteger();
            }
            else if (valueType.IsDecimal())
            {
                // decimal
                decimalSummary += caseValue.Value.JsonToDecimal();
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        return ValueConvert.ToJson(valueType == ValueType.Integer ? intSummary : decimalSummary);
    }
}