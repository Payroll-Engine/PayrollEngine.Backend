
namespace PayrollEngine.Domain.Model;

/// <summary>Case value calculator by effective month working day</summary>
public class MonthEffectiveWorkDayPayrollCalculator : MonthWorkDayPayrollCalculator
{
    /// <inheritdoc />
    public MonthEffectiveWorkDayPayrollCalculator(IPayrollCalendar calendar) :
        base(calendar)
    {
    }

    /// <inheritdoc />
    protected override decimal? CalculateValue(CaseValueCalculation calculation) =>
        CalculatePeriodValue(calculation, calculation.EvaluationPeriod.GetWorkingDaysCount(WorkingDays));
}