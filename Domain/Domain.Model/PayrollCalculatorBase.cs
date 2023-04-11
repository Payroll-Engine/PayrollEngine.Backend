using System;

namespace PayrollEngine.Domain.Model;

/// <inheritdoc />
public abstract class PayrollCalculatorBase : IPayrollCalculator
{
    /// <summary>The payroll calendar</summary>
    public IPayrollCalendar Calendar { get; }

    /// <summary>Initializes a new instance of the <see cref="PayrollCalculatorBase"/> class</summary>
    /// <param name="calendar">The payroll calendar</param>
    protected PayrollCalculatorBase(IPayrollCalendar calendar)
    {
        Calendar = calendar ?? throw new ArgumentNullException(nameof(calendar));
    }

    /// <inheritdoc />
    public abstract IPayrollPeriod GetPayrunPeriod(DateTime periodMoment);

    /// <inheritdoc />
    public abstract IPayrollPeriod GetPayrunCycle(DateTime cycleMoment);

    /// <inheritdoc />
    public decimal? CalculateCasePeriodValue(CaseValueCalculation calculation) =>
        CalculateValue(calculation);

    /// <summary>Custom value calculation</summary>
    /// <param name="calculation">The calculation</param>
    /// <returns>The case period value</returns>
    protected abstract decimal? CalculateValue(CaseValueCalculation calculation);
}