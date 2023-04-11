
namespace PayrollEngine.Domain.Model;

/// <summary>Case value calculator by month calendar days</summary>
public class MonthCalendarDayPayrollCalculator : MonthDayPayrollCalculator
{
    /// <inheritdoc />
    public MonthCalendarDayPayrollCalculator(IPayrollCalendar calendar) :
        base(calendar)
    {
    }

    /// <inheritdoc />
    protected override decimal? CalculateValue(CaseValueCalculation calculation) =>
        CalculatePeriodValue(calculation, Date.DaysInMonth(calculation.EvaluationPeriod.Start));
}