using System;
using System.Collections.Generic;
using System.Linq;

namespace PayrollEngine.Domain.Model;

/// <summary>Case value calculator by week calendar days</summary>
public class WeekDayPayrollCalculator : PayrollCalculatorBase
{
    /// <summary>List of working days</summary>
    public IList<DayOfWeek> WorkingDays { get; }

    /// <inheritdoc />
    public WeekDayPayrollCalculator(IPayrollCalendar calendar) :
        base(calendar)
    {
        if (!calendar.Configuration.WorkingDays.Contains(calendar.Configuration.FirstDayOfWeek))
        {
            throw new PayrollException($"First day of week {calendar.Configuration.FirstDayOfWeek} is not included within the working days {string.Join(",", calendar.Configuration.WorkingDays)}");
        }
        WorkingDays = calendar.Configuration.WorkingDays ?? new List<DayOfWeek>(Enum.GetValues(typeof(DayOfWeek)).OfType<DayOfWeek>());
    }

    /// <inheritdoc />
    public override IPayrollPeriod GetPayrunPeriod(DateTime periodMoment) =>
        new WeekPayrollPeriod(Calendar, periodMoment);

    /// <inheritdoc />
    public override IPayrollPeriod GetPayrunCycle(DateTime cycleMoment) =>
        new YearPayrollCycle(Calendar, cycleMoment);

    /// <inheritdoc />
    protected override decimal? CalculateValue(CaseValueCalculation calculation)
    {
        // working days
        decimal workingDaysCount = calculation.CaseValuePeriod.GetWorkingDaysCount(WorkingDays);
        if (workingDaysCount <= 0)
        {
            return null;
        }

        // divide base value by week days
        var value = calculation.CaseValue / Date.DaysInWeek * workingDaysCount;

        // result
        return value;
    }
}