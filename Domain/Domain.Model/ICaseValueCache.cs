using System.Collections.Generic;
using System.Threading.Tasks;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Cache for case values for a specific evaluation date and period
/// </summary>
public interface ICaseValueCache
{
    /// <summary>
    /// Get all case value slots
    /// </summary>
    /// <param name="caseFieldName">The case field name</param>
    /// <returns>The case value slots</returns>
    Task<IEnumerable<string>> GetCaseValueSlotsAsync(string caseFieldName);

    /// <summary>
    /// Get all case values before the evaluation date
    /// </summary>
    /// <param name="caseFieldName">The case field name</param>
    /// <returns>The case values</returns>
    Task<IEnumerable<CaseValue>> GetCaseValuesAsync(string caseFieldName);

    /// <summary>
    /// Get all case values before the evaluation date and withing a certain time period
    /// </summary>
    /// <param name="caseFieldName">The case field name</param>
    /// <returns>The case period values</returns>
    Task<IEnumerable<CaseValue>> GetCasePeriodValuesAsync(string caseFieldName);

    /// <summary>
    /// Get retro case values from a certain time period
    /// </summary>
    /// <param name="caseFieldName">The case field name</param>
    /// <param name="period">The date period</param>
    /// <returns>The retro case value</returns>
    Task<CaseValue> GetRetroCaseValueAsync(string caseFieldName, DatePeriod period);
}