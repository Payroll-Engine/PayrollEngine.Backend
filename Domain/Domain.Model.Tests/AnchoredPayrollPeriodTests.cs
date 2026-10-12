using System;
using System.Globalization;
using Xunit;

namespace PayrollEngine.Domain.Model.Tests;

public class AnchoredPayrollPeriodTests
{
    [Fact]
    public void BiWeekAnchoredPeriodTest()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar { PeriodAnchorDate = new DateTime(2026, 1, 2) };
        var period = new BiWeekPayrollPeriod(culture, calendar, new DateTime(2026, 1, 20));
        Assert.Equal(new DateTime(2026, 1, 16), period.Start);
        Assert.Equal(new DateTime(2026, 1, 30).AddTicks(-1), period.End);
    }

    [Fact]
    public void BiWeekAnchoredPeriodYearCrossingTest()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar { PeriodAnchorDate = new DateTime(2025, 1, 3) };
        var period = new BiWeekPayrollPeriod(culture, calendar, new DateTime(2026, 1, 1));
        Assert.Equal(new DateTime(2025, 12, 19), period.Start);
        Assert.Equal(new DateTime(2026, 1, 2).AddTicks(-1), period.End);
    }

    [Fact]
    public void BiWeekAnchoredPeriodBeforeAnchorTest()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar { PeriodAnchorDate = new DateTime(2026, 1, 9) };
        var period = new BiWeekPayrollPeriod(culture, calendar, new DateTime(2026, 1, 1));
        Assert.Equal(new DateTime(2025, 12, 26), period.Start);
        Assert.Equal(new DateTime(2026, 1, 9).AddTicks(-1), period.End);
    }

    [Fact]
    public void BiWeekAnchoredPeriodsContinuousTest()
    {
        var culture = new CultureInfo("en-US");

        // 2026 has 53 calendar weeks: the anchored periods stay continuous over the year ends
        var calendar = new Calendar { PeriodAnchorDate = new DateTime(2025, 1, 3) };
        IPayrollPeriod period = new BiWeekPayrollPeriod(culture, calendar, new DateTime(2025, 1, 3));
        for (var i = 0; i < 80; i++)
        {
            var next = period.GetPayrollPeriod(period.Start, 1);
            Assert.Equal(period.End.AddTicks(1), next.Start);
            Assert.Equal(new DateTime(2025, 1, 3).AddDays((i + 1) * 14), next.Start);
            period = next;
        }
    }

    [Fact]
    public void BiWeekPeriodWithoutAnchorTest()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar();
        var period = new BiWeekPayrollPeriod(culture, calendar, new DateTime(2023, 1, 18));
        Assert.Equal(new DateTime(2023, 1, 15), period.Start);
        Assert.Equal(new DateTime(2023, 1, 29).AddTicks(-1), period.End);
    }

    [Fact]
    public void WeekAnchoredPeriodTest()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar { PeriodAnchorDate = new DateTime(2026, 1, 2) };
        var period = new WeekPayrollPeriod(culture, calendar, new DateTime(2026, 1, 8));
        Assert.Equal(new DateTime(2026, 1, 2), period.Start);
        Assert.Equal(new DateTime(2026, 1, 9).AddTicks(-1), period.End);

        var next = new WeekPayrollPeriod(culture, calendar, new DateTime(2026, 1, 9));
        Assert.Equal(new DateTime(2026, 1, 9), next.Start);
    }

    [Fact]
    public void PayDateMonthOffsetTest()
    {
        var calendar = new Calendar
        {
            CycleTimeUnit = CalendarTimeUnit.Year,
            PeriodTimeUnit = CalendarTimeUnit.CalendarMonth,
            PayDateOffset = 3
        };
        var calculator = new PayrollCalculator(new CultureInfo("en-US"), calendar);
        Assert.Equal(new DateTime(2026, 1, 3), calculator.GetPayDate(new DateTime(2025, 12, 15)));
    }

    [Fact]
    public void PayDateWithoutOffsetTest()
    {
        var calendar = new Calendar
        {
            CycleTimeUnit = CalendarTimeUnit.Year,
            PeriodTimeUnit = CalendarTimeUnit.CalendarMonth
        };
        var calculator = new PayrollCalculator(new CultureInfo("en-US"), calendar);
        Assert.Equal(new DateTime(2025, 12, 31), calculator.GetPayDate(new DateTime(2025, 12, 15)));
    }

    [Fact]
    public void PayDateBiWeekAnchoredTest()
    {
        var calendar = new Calendar
        {
            CycleTimeUnit = CalendarTimeUnit.Year,
            PeriodTimeUnit = CalendarTimeUnit.BiWeek,
            PeriodAnchorDate = new DateTime(2025, 1, 3),
            PayDateOffset = 6
        };
        var calculator = new PayrollCalculator(new CultureInfo("en-US"), calendar);
        Assert.Equal(new DateTime(2026, 1, 7), calculator.GetPayDate(new DateTime(2025, 12, 25)));
    }

    [Fact]
    public void RegulationDateTest()
    {
        var periodEnd = new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc).LastMomentOfDay();

        var samePayDate = new PayrunJob { PeriodEnd = periodEnd, PayDate = new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc) };
        Assert.Equal(periodEnd, samePayDate.RegulationDate);

        var laterPayDate = new PayrunJob { PeriodEnd = periodEnd, PayDate = new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc) };
        Assert.Equal(new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc).LastMomentOfDay(), laterPayDate.RegulationDate);

        var legacyJob = new PayrunJob { PeriodEnd = periodEnd };
        Assert.Equal(periodEnd, legacyJob.RegulationDate);
    }
}
