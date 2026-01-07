
namespace PayrollEngine.Domain.Model;

/// <summary>
/// Extensions for case values
/// </summary>
public static class CaseValueExtensions
{
    /// <param name="caseValue">The case value</param>
    extension(CaseValue caseValue)
    {
        /// <summary>
        /// Get case value date period
        /// </summary>
        /// <returns>The date period</returns>
        public DatePeriod GetPeriod() =>
            new(caseValue.Start, caseValue.End);

        /// <summary>
        /// Get case value reference
        /// </summary>
        /// <returns>The case value reference</returns>
        public string GetCaseValueReference()
        {
            if (caseValue == null)
            {
                return null;
            }
            return CaseValueReference.ToReference(caseValue.CaseFieldName, caseValue.CaseSlot);
        }
    }
}