using System;
using System.Collections.Generic;
using System.Linq;

namespace PayrollEngine.Domain.Model;

/// <summary>Case value calculator by month calendar days</summary>
public abstract class MonthWorkDayPayrollCalculator : MonthPayrollCalculator
{
    /// <summary>List of working days</summary>
    public IList<DayOfWeek> WorkingDays { get; }

    /// <inheritdoc />
    protected MonthWorkDayPayrollCalculator(IPayrollCalendar calendar) :
        base(calendar)
    {
        WorkingDays = calendar.Configuration.WorkingDays ?? new List<DayOfWeek>(Enum.GetValues(typeof(DayOfWeek)).OfType<DayOfWeek>());
    }

    /// <summary>Calculate period value by days in month</summary>
    /// <param name="calculation">The calculation</param>
    /// <param name="daysInMonth">The number of days in the month</param>
    /// <returns>The period value</returns>
    protected decimal? CalculatePeriodValue(CaseValueCalculation calculation, decimal daysInMonth)
    {
        if (!calculation.EvaluationPeriod.Start.IsSameMonth(calculation.EvaluationPeriod.End))
        {
            throw new PayrollException($"Evaluation period {calculation.EvaluationPeriod} must be within a month");
        }

        // working days
        decimal workingDaysCount = calculation.CaseValuePeriod.GetWorkingDaysCount(WorkingDays);
        if (workingDaysCount <= 0)
        {
            return null;
        }

        // divide base value by month working days
        var value = calculation.CaseValue / daysInMonth * workingDaysCount;

        // result
        return value;
    }
}