
namespace PayrollEngine.Domain.Model;

/// <summary>
/// Extensions for case values
/// </summary>
public static class CaseValueExtensions
{
    /// <summary>
    /// Get case value date period
    /// </summary>
    /// <param name="caseValue">The case value</param>
    /// <returns>The date period</returns>
    public static DatePeriod GetPeriod(this CaseValue caseValue) =>
        new(caseValue.Start, caseValue.End);

    /// <summary>
    /// Get case value reference
    /// </summary>
    /// <param name="caseValue">The case value</param>
    /// <returns>The case value reference</returns>
    public static string GetCaseValueReference(this CaseValue caseValue)
    {
        if (caseValue == null)
        {
            return null;
        }
        return CaseValueReference.ToReference(caseValue.CaseFieldName, caseValue.CaseSlot);
    }
}