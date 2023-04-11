
namespace PayrollEngine.Domain.Model;

/// <summary>Case value calculator by month calendar days</summary>
public abstract class MonthDayPayrollCalculator : MonthPayrollCalculator
{
    /// <inheritdoc />
    protected MonthDayPayrollCalculator(IPayrollCalendar calendar) :
        base(calendar)
    {
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

        // end of day
        var periodEnd = calculation.CaseValuePeriod.End.RoundLastMoment();
        var duration = new DatePeriod(calculation.CaseValuePeriod.Start.Date, periodEnd).Duration;

        // ensure at least one day
        var periodDays = (decimal)duration.TotalDays;
        if (periodDays < 1)
        {
            return null;
        }

        // divide base value by month days
        var value = calculation.CaseValue / daysInMonth * periodDays;

        // result
        return value;
    }
}