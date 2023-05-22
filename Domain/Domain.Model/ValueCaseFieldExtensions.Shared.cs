
namespace PayrollEngine.Domain.Model;

/// <summary>Extension methods for the <see cref="CaseFieldSet"/></summary>
public static class CaseFieldSetExtensions
{
    /// <summary>Test for complete case field</summary>
    /// <param name="caseFieldSet">The derived case field to test</param>
    /// <returns>True if the case field is complete,  otherwise false</returns>
    public static bool IsComplete(this CaseFieldSet caseFieldSet)
    {
        if (caseFieldSet == null)
        {
            return false;
        }

        if (caseFieldSet.ValueType == ValueType.None)
        {
            // no values at all
            return true;
        }

        var hasValue = caseFieldSet.HasValue;

        // timeless
        if (caseFieldSet.TimeType == CaseFieldTimeType.Timeless)
        {
            return !caseFieldSet.ValueMandatory || hasValue;
        }

        // timed value
        var hasStart = caseFieldSet.Start.HasValue;
        if (!caseFieldSet.ValueMandatory)
        {
            // optional field: both set or unset
            return hasStart == hasValue;
        }

        // mandatory field with start and value
        return hasStart && hasValue;
    }
}