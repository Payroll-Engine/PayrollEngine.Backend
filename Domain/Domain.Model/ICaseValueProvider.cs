using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Provides a case value
/// </summary>
public interface ICaseValueProvider
{
    /// <summary>
    /// The employee
    /// </summary>
    Employee Employee { get; }

    /// <summary>
    /// The case field provider
    /// </summary>
    ICaseFieldProvider CaseFieldProvider { get; }

    /// <summary>
    /// The evaluation date
    /// </summary>
    DateTime EvaluationDate { get; }

    #region Payroll Calculator
    /// <summary>
    /// The current payroll calculator
    /// </summary>
    IPayrollCalculator PayrollCalculator { get; }

    /// <summary>
    /// Push the current payroll calculator
    /// </summary>
    /// <param name="payrollCalculator">The new payroll calculator</param>
    void PushCalculator(IPayrollCalculator payrollCalculator);

    /// <summary>
    /// Pop the current payroll calculator
    /// </summary>
    /// <param name="payrollCalculator">The new payroll calculator</param>
    void PopCalculator(IPayrollCalculator payrollCalculator);

    #endregion

    #region Periods

    /// <summary>
    /// The current evaluation period
    /// </summary>
    DatePeriod EvaluationPeriod { get; }

    #endregion

    #region Case Value

    /// <summary>
    /// Get all case slots from a specific case field
    /// </summary>
    /// <param name="caseFieldName">The case field name</param>
    /// <returns>The case values</returns>
    Task<IEnumerable<string>> GetCaseValueSlotsAsync(string caseFieldName);

    /// <summary>
    /// Get all case values (only active objects) by case type
    /// </summary>
    /// <param name="valueDate">The value date</param>
    /// <param name="caseType">The case type</param>
    /// <returns>The case value at a given time, null if no value is available</returns>
    Task<List<CaseValue>> GetTimeCaseValuesAsync(DateTime valueDate, CaseType caseType);

    /// <summary>
    /// Get case values (only active objects) from a specific time
    /// </summary>
    /// <param name="valueDate">The value date</param>
    /// <param name="caseType">The case type</param>
    /// <param name="caseFieldNames">The case field names</param>
    /// <returns>The case value at a given time, null if no value is available</returns>
    Task<List<CaseValue>> GetTimeCaseValuesAsync(DateTime valueDate, CaseType caseType,
        IEnumerable<string> caseFieldNames);

    /// <summary>
    /// Get case values (only active objects)
    /// </summary>
    /// <param name="caseFieldName">The case field name</param>
    /// <param name="evaluationPeriod">The evaluation period</param>
    /// <param name="caseSlot">The case slot</param>
    /// <returns>The case value periods for a time period</returns>
    Task<List<CaseFieldValue>> GetCaseValuesAsync(string caseFieldName, DatePeriod evaluationPeriod,
        string caseSlot = null);

    /// <summary>
    /// Get case value by split periods
    /// </summary>
    /// <param name="caseFieldName">The case field name</param>
    /// <param name="caseType">The case type</param>
    /// <param name="caseSlot">The case slot</param>
    /// <returns>case values with value periods</returns>
    Task<IDictionary<CaseValue, List<DatePeriod>>> GetCaseValueSplitPeriodsAsync(
        string caseFieldName, CaseType caseType, string caseSlot = null);

    #endregion

    #region Period Values

    /// <summary>
    /// Get case period values by date period and the case field names
    /// </summary>
    /// <param name="period">The date period</param>
    /// <param name="caseFieldNames">The case field names</param>
    /// <returns>The case values for all case fields</returns>
    Task<IList<CaseFieldValue>> GetCasePeriodValuesAsync(
        DatePeriod period, IEnumerable<string> caseFieldNames);

    #endregion

    #region Retro

    /// <summary>
    /// Gets the retro case value
    /// </summary>
    CaseValue RetroCaseValue { get; }

    #endregion
}

