using System.Collections.Generic;
using System.Linq;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Extension methods for the <see cref="Case"/>
/// </summary>
public static class CaseExtensions
{
    /// <summary>
    /// Validate all types from derived case
    /// </summary>
    /// <param name="derivedCase">The derived case to validate</param>
    /// <returns>throws a payroll exception if the derived case is invalid</returns>
    public static void ValidateDerivedTypes(this IList<Case> derivedCase)
    {
        if (derivedCase == null)
        {
            return;
        }

        var refCase = derivedCase.First();
        foreach (var @case in derivedCase)
        {
            // case type
            if (@case.CaseType != refCase.CaseType)
            {
                throw new PayrollException($"Case {refCase.Name}: different case types in case #{@case.Id} ({@case.CaseType}) and case #{refCase.Id} ({refCase.CaseType})");
            }
        }
    }
}