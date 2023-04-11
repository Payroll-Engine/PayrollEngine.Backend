using System;

namespace PayrollEngine.Domain.Model;

/// <summary>Case value calculator by average working days</summary>
/// TODO: check case when month average is 30 days and two periods with total 31 days -> create 100%+ values
public class MonthAverageWorkDayPayrollCalculator : MonthWorkDayPayrollCalculator
{
    /// <summary>Average work days</summary>
    public decimal AverageWorkDays { get; }

    /// <inheritdoc />
    public MonthAverageWorkDayPayrollCalculator(IPayrollCalendar calendar) :
        base(calendar)
    {
        if (calendar.Configuration.AverageWorkDays <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(calendar.Configuration.AverageWorkDays));
        }
        AverageWorkDays = calendar.Configuration.AverageWorkDays;
    }

    /// <inheritdoc />
    protected override decimal? CalculateValue(CaseValueCalculation calculation) =>
        CalculatePeriodValue(calculation, AverageWorkDays);
}