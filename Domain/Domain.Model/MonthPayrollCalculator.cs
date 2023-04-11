using System;

namespace PayrollEngine.Domain.Model;

/// <summary>Case value calculator by month calendar days</summary>
public abstract class MonthPayrollCalculator : PayrollCalculatorBase
{
    /// <inheritdoc />
    protected MonthPayrollCalculator(IPayrollCalendar calendar) :
        base(calendar)
    {
    }

    /// <inheritdoc />
    public override IPayrollPeriod GetPayrunPeriod(DateTime periodMoment) =>
        new MonthPayrollPeriod(Calendar, periodMoment);

    /// <inheritdoc />
    public override IPayrollPeriod GetPayrunCycle(DateTime cycleMoment) =>
        new YearPayrollCycle(Calendar, cycleMoment);
}