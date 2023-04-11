using System;

namespace PayrollEngine.Domain.Model;

/// <summary>Case value calculator by month average days (e.g. 30 days)</summary>
public class MonthAverageDayPayrollCalculator : MonthDayPayrollCalculator
{
    /// <summary>Average month days</summary>
    public decimal AverageMonthDays { get; }

    /// <inheritdoc />
    public MonthAverageDayPayrollCalculator(IPayrollCalendar calendar) :
        base(calendar)
    {
        if (calendar.Configuration.AverageMonthDays <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(calendar.Configuration.AverageMonthDays));
        }
        AverageMonthDays = calendar.Configuration.AverageMonthDays;
    }

    /// <inheritdoc />
    protected override decimal? CalculateValue(CaseValueCalculation calculation) =>
        CalculatePeriodValue(calculation, AverageMonthDays);
}