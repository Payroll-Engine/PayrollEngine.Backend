using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Assignment of payrun periods to cycles, based on the period cycle date:
/// the period end, or the pay date of the period
/// </summary>
/// <remarks>The pay date of a period is the pay date of the latest full payrun job,
/// or the calendar pay date for periods without payrun job</remarks>
public sealed class PayrunCyclePeriods
{
    private const int MaxPeriodCount = 10000;
    private readonly IReadOnlyDictionary<DateTime, DateTime> periodPayDates;

    /// <summary>Constructor</summary>
    /// <param name="periodPayDates">The job pay dates by period start</param>
    public PayrunCyclePeriods(IReadOnlyDictionary<DateTime, DateTime> periodPayDates = null)
    {
        this.periodPayDates = periodPayDates ?? new Dictionary<DateTime, DateTime>();
    }

    /// <summary>
    /// Test if the cycle date of a period is at or after a moment
    /// </summary>
    /// <param name="calculator">The payroll calculator</param>
    /// <param name="period">The payrun period</param>
    /// <param name="moment">The test moment</param>
    public bool IsCycleDateAtOrAfter(IPayrollCalculator calculator, IPayrollPeriod period, DateTime moment) =>
        calculator.CycleAssignment == CalendarCycleAssignment.PeriodEnd
            ? period.End >= moment
            : GetPayDate(calculator, period) >= moment.Date;

    /// <summary>
    /// Get the pay date of a period: the job pay date or the calendar pay date
    /// </summary>
    /// <param name="calculator">The payroll calculator</param>
    /// <param name="period">The payrun period</param>
    public DateTime GetPayDate(IPayrollCalculator calculator, IPayrollPeriod period) =>
        periodPayDates.TryGetValue(period.Start, out var jobPayDate)
            ? jobPayDate.Date
            : calculator.GetPayDate(period.Start).Date;

    /// <summary>
    /// Get the start of the first period with a cycle date at or after a moment
    /// </summary>
    /// <param name="calculator">The payroll calculator</param>
    /// <param name="moment">The moment, typically the cycle start</param>
    public DateTime GetFirstPeriodStart(IPayrollCalculator calculator, DateTime moment)
    {
        var period = calculator.GetPayrunPeriod(calculator.GetCyclePeriodStart(moment));
        if (calculator.CycleAssignment == CalendarCycleAssignment.PeriodEnd)
        {
            return period.Start;
        }

        // early payment of the period
        var count = 0;
        while (count++ < MaxPeriodCount && !IsCycleDateAtOrAfter(calculator, period, moment))
        {
            var next = period.GetPayrollPeriod(period.Start, 1);
            if (next.Start <= period.Start)
            {
                break;
            }
            period = next;
        }

        // late payment of previous periods
        var firstPeriodStart = period.Start;
        foreach (var periodPayDate in periodPayDates)
        {
            if (periodPayDate.Key < firstPeriodStart && periodPayDate.Value.Date >= moment.Date)
            {
                firstPeriodStart = calculator.GetPayrunPeriod(periodPayDate.Key).Start;
            }
        }
        return firstPeriodStart;
    }

    /// <summary>
    /// Get the consolidated period starts: all periods before the job period
    /// with a cycle date at or after a moment, with pay date assignment
    /// limited to periods paid until the job pay date
    /// </summary>
    /// <param name="calculator">The payroll calculator</param>
    /// <param name="periodMoment">The moment, typically the cycle start</param>
    /// <param name="jobPeriodStart">The job period start</param>
    /// <returns>List of period start dates</returns>
    public List<DateTime> GetConsolidatedPeriodStarts(IPayrollCalculator calculator,
        DateTime periodMoment, DateTime jobPeriodStart)
    {
        var payDateAssignment = calculator.CycleAssignment == CalendarCycleAssignment.PayDate;
        var jobPayDate = GetPayDate(calculator, calculator.GetPayrunPeriod(jobPeriodStart));

        var periodStarts = new List<DateTime>();
        var period = calculator.GetPayrunPeriod(GetFirstPeriodStart(calculator, periodMoment));
        var count = 0;
        // iterate from the start period until the job period
        while (count++ < MaxPeriodCount && period.Start < jobPeriodStart)
        {
            if (IsCycleDateAtOrAfter(calculator, period, periodMoment) &&
                (!payDateAssignment || GetPayDate(calculator, period) <= jobPayDate))
            {
                periodStarts.Add(period.Start);
            }
            // next period
            var next = period.GetPayrollPeriod(period.Start, 1);
            // defensive guard: if the period did not advance, the calendar implementation
            // has a bug (e.g. GetPayrollPeriod uses an incorrect step size). Break to
            // prevent an infinite loop. The affected period type will be reported in logs.
            if (next.Start <= period.Start)
            {
                PayrollEngine.Log.Warning("GetConsolidatedPeriodStarts: period did not advance " +
                            $"(Start={period.Start:yyyy-MM-dd}, PeriodMoment={periodMoment:yyyy-MM-dd}, " +
                            $"JobPeriodStart={jobPeriodStart:yyyy-MM-dd}). " +
                            "Breaking consolidation loop — check IPayrollPeriod.GetPayrollPeriod implementation.");
                break;
            }
            period = next;
        }
        return periodStarts;
    }
}
