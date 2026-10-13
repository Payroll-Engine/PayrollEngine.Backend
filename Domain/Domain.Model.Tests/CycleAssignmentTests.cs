using System;
using System.Collections.Generic;
using System.Globalization;
using Xunit;

namespace PayrollEngine.Domain.Model.Tests;

public class CycleAssignmentTests
{
    private static DateTime Utc(int year, int month, int day) =>
        new(year, month, day, 0, 0, 0, DateTimeKind.Utc);

    private static PayrollCalculator MonthCalculator(int payDateOffset, CalendarCycleAssignment? assignment = null) =>
        new(new CultureInfo("en-US"), new Calendar
        {
            CycleTimeUnit = CalendarTimeUnit.Year,
            PeriodTimeUnit = CalendarTimeUnit.CalendarMonth,
            PayDateOffset = payDateOffset,
            CycleAssignment = assignment
        });

    private static PayrollCalculator BiWeekCalculator(CalendarCycleAssignment? assignment = null) =>
        new(new CultureInfo("en-US"), new Calendar
        {
            CycleTimeUnit = CalendarTimeUnit.Year,
            PeriodTimeUnit = CalendarTimeUnit.BiWeek,
            PeriodAnchorDate = Utc(2025, 12, 28),
            CycleAssignment = assignment
        });

    private static PayrunJob CreateJob(PayrollCalculator calculator, DateTime periodStart, DateTime? payDate = null) =>
        PayrunJobFactory.CreatePayrunJob(new PayrunJobInvocation
        {
            PeriodStart = periodStart,
            PayDate = payDate,
            EvaluationDate = Utc(2027, 2, 1)
        }, payrunId: 1, userId: 1, divisionId: 1, payrollId: 1, calculator);

    [Fact]
    public void DefaultAssignmentIsPayDateTest()
    {
        Assert.Equal(CalendarCycleAssignment.PayDate, MonthCalculator(0).CycleAssignment);
    }

    [Fact]
    public void JobCycleByPayDateTest()
    {
        // December period paid in January: cycle of the pay date
        var job = CreateJob(MonthCalculator(5), Utc(2026, 12, 1));
        Assert.Equal(Utc(2027, 1, 5), job.PayDate);
        Assert.Equal(Utc(2027, 1, 1), job.CycleStart);
        Assert.Equal(job.PayDate.LastMomentOfDay(), job.RegulationDate);
    }

    [Fact]
    public void JobCycleByPeriodEndTest()
    {
        // December period paid in January: cycle of the period end
        var job = CreateJob(MonthCalculator(5, CalendarCycleAssignment.PeriodEnd), Utc(2026, 12, 1));
        Assert.Equal(Utc(2027, 1, 5), job.PayDate);
        Assert.Equal(Utc(2026, 1, 1), job.CycleStart);
        Assert.Equal(job.PeriodEnd, job.RegulationDate);
    }

    [Fact]
    public void JobCyclePayDateOverrideTest()
    {
        // early payment of the December period within December
        var job = CreateJob(MonthCalculator(5), Utc(2026, 12, 1), Utc(2026, 12, 31));
        Assert.Equal(Utc(2026, 1, 1), job.CycleStart);
    }

    [Fact]
    public void JobCycleInvocationWithinPeriodTest()
    {
        // the invocation period start can be any date within the period
        var calculator = BiWeekCalculator();
        var job = CreateJob(calculator, Utc(2026, 12, 31));
        Assert.Equal(Utc(2026, 12, 27), job.PeriodStart);
        Assert.Equal(Utc(2027, 1, 1), job.CycleStart);
    }

    [Fact]
    public void CyclePeriodStartTest()
    {
        // pay date: the December period paid in January is the first period of the cycle
        Assert.Equal(Utc(2025, 12, 1), MonthCalculator(5).GetCyclePeriodStart(Utc(2026, 1, 1)));
        Assert.Equal(Utc(2026, 1, 1), MonthCalculator(0).GetCyclePeriodStart(Utc(2026, 1, 1)));
        Assert.Equal(Utc(2026, 1, 1), MonthCalculator(5, CalendarCycleAssignment.PeriodEnd).GetCyclePeriodStart(Utc(2026, 1, 1)));

        // bi-week period Dec 28, 2025 - Jan 10, 2026
        Assert.Equal(Utc(2025, 12, 28), BiWeekCalculator().GetCyclePeriodStart(Utc(2026, 1, 1)));
        Assert.Equal(Utc(2025, 12, 28), BiWeekCalculator(CalendarCycleAssignment.PeriodEnd).GetCyclePeriodStart(Utc(2026, 1, 1)));
    }

    [Fact]
    public void ConsolidatedPeriodStartsByPayDateTest()
    {
        var calculator = MonthCalculator(5);
        var periods = new PayrunCyclePeriods();
        var periodStarts = periods.GetConsolidatedPeriodStarts(calculator, Utc(2027, 1, 1), Utc(2027, 3, 1));
        Assert.Equal([Utc(2026, 12, 1), Utc(2027, 1, 1), Utc(2027, 2, 1)], periodStarts);
    }

    [Fact]
    public void ConsolidatedPeriodStartsByPeriodEndTest()
    {
        var calculator = MonthCalculator(5, CalendarCycleAssignment.PeriodEnd);
        var periods = new PayrunCyclePeriods();
        var periodStarts = periods.GetConsolidatedPeriodStarts(calculator, Utc(2027, 1, 1), Utc(2027, 3, 1));
        Assert.Equal([Utc(2027, 1, 1), Utc(2027, 2, 1)], periodStarts);
    }

    [Fact]
    public void ConsolidatedPeriodStartsEarlyPaymentTest()
    {
        // December period paid on December 31 instead of January 5
        var calculator = MonthCalculator(5);
        var periods = new PayrunCyclePeriods(new Dictionary<DateTime, DateTime>
        {
            { Utc(2026, 12, 1), Utc(2026, 12, 31) }
        });
        Assert.Equal(Utc(2027, 1, 1), periods.GetFirstPeriodStart(calculator, Utc(2027, 1, 1)));
        Assert.Equal([Utc(2027, 1, 1)], periods.GetConsolidatedPeriodStarts(calculator, Utc(2027, 1, 1), Utc(2027, 2, 1)));
        Assert.Contains(Utc(2026, 12, 1), periods.GetConsolidatedPeriodStarts(calculator, Utc(2026, 1, 1), Utc(2027, 1, 1)));
    }

    [Fact]
    public void ConsolidatedPeriodStartsLatePaymentTest()
    {
        // November period paid on January 2 without pay date offset
        var calculator = MonthCalculator(0);
        var periods = new PayrunCyclePeriods(new Dictionary<DateTime, DateTime>
        {
            { Utc(2026, 11, 1), Utc(2027, 1, 2) }
        });
        Assert.Equal([Utc(2026, 11, 1), Utc(2027, 1, 1)],
            periods.GetConsolidatedPeriodStarts(calculator, Utc(2027, 1, 1), Utc(2027, 2, 1)));
        Assert.DoesNotContain(Utc(2026, 11, 1),
            periods.GetConsolidatedPeriodStarts(calculator, Utc(2026, 1, 1), Utc(2026, 12, 1)));
    }

    [Fact]
    public void ConsolidatedPeriodStartsBiWeekYearChangeTest()
    {
        // the period Dec 28, 2025 - Jan 10, 2026 belongs to 2026
        var calculator = BiWeekCalculator();
        var periods = new PayrunCyclePeriods();
        var periodStarts = periods.GetConsolidatedPeriodStarts(calculator, Utc(2026, 1, 1), Utc(2026, 1, 25));
        Assert.Equal([Utc(2025, 12, 28), Utc(2026, 1, 11)], periodStarts);
    }

    [Fact]
    public void RegulationDatePeriodEndAssignmentTest()
    {
        var periodEnd = Utc(2026, 12, 31).LastMomentOfDay();
        var job = new PayrunJob
        {
            PeriodEnd = periodEnd,
            PayDate = Utc(2027, 1, 5),
            CycleAssignment = CalendarCycleAssignment.PeriodEnd
        };
        Assert.Equal(periodEnd, job.RegulationDate);
    }
}
