using System.Collections.Generic;
using System.Linq;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Extension methods for the <see cref="CaseFieldSet"/>
/// </summary>
public static class CaseFieldExtensions
{
    /// <summary>
    /// Validate all types from derived case field
    /// </summary>
    /// <param name="derivedCaseField">The derived case field to validate</param>
    /// <returns>throws a payroll exception if the derived case field is invalid</returns>
    public static void ValidateDerivedTypes(this IList<CaseField> derivedCaseField)
    {
        if (derivedCaseField == null)
        {
            return;
        }

        var refCaseField = derivedCaseField.First();
        foreach (var caseField in derivedCaseField)
        {
            // value type
            if (caseField.ValueType != refCaseField.ValueType)
            {
                throw new PayrollException($"Case field {refCaseField.Name}: different value types in case field #{caseField.Id} ({caseField.ValueType}) " +
                                           $"and case field #{refCaseField.Id} ({refCaseField.ValueType})");
            }
            // time type
            if (caseField.TimeType != refCaseField.TimeType)
            {
                throw new PayrollException($"Case field {refCaseField.Name}: different time types in case field #{caseField.Id} ({caseField.TimeType}) " +
                                           $"and case field #{refCaseField.Id} ({refCaseField.TimeType})");
            }
            // time unit
            if (caseField.TimeUnit != refCaseField.TimeUnit)
            {
                throw new PayrollException($"Case field {refCaseField.Name}: different time units in case field #{caseField.Id} ({caseField.TimeUnit}) " +
                                           $"and case field #{refCaseField.Id} ({refCaseField.TimeUnit})");
            }
            // optional
            if (caseField.ValueMandatory != refCaseField.ValueMandatory)
            {
                throw new PayrollException($"Case field {refCaseField.Name}: different value mandatory state in case field #{caseField.Id} ({caseField.TimeUnit}) " +
                                           $"and case field #{refCaseField.Id} ({refCaseField.TimeUnit})");
            }
        }
    }
}