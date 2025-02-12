using System;
using System.Globalization;
using Xunit;

namespace PayrollEngine.Domain.Model.Tests;

public class BiWeekPayrollPeriodTests
{
    [Fact]
    public void BiWeekPayrollYearFirst1Test()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar();
        var year = new BiWeekPayrollPeriod(culture, calendar, new DateTime(2023, 1, 1));
        Assert.Equal(new DateTime(2023, 1, 1), year.Start);
        Assert.Equal(new DateTime(2023, 1, 15).AddTicks(-1), year.End);
    }

    [Fact]
    public void BiWeekPayrollYearFirst2Test()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar();
        var year = new BiWeekPayrollPeriod(culture, calendar, new DateTime(2023, 1, 8));
        Assert.Equal(new DateTime(2023, 1, 1), year.Start);
        Assert.Equal(new DateTime(2023, 1, 15).AddTicks(-1), year.End);
    }

    [Fact]
    public void BiWeekPayrollYearSecondTest()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar();
        var year = new BiWeekPayrollPeriod(culture, calendar, new DateTime(2023, 1, 18));
        Assert.Equal(new DateTime(2023, 1, 15), year.Start);
        Assert.Equal(new DateTime(2023, 1, 29).AddTicks(-1), year.End);
    }

    [Fact]
    public void BiWeekPayrollYearSecondLastTest()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar();
        var year = new BiWeekPayrollPeriod(culture, calendar, new DateTime(2023, 12, 28));
        Assert.Equal(new DateTime(2023, 12, 17), year.Start);
        Assert.Equal(new DateTime(2023, 12, 31).AddTicks(-1), year.End);
    }

    [Fact]
    public void BiWeekPayrollYearLastTest()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar();
        var year = new BiWeekPayrollPeriod(culture, calendar, new DateTime(2023, 12, 31));
        Assert.Equal(new DateTime(2023, 12, 31), year.Start);
        Assert.Equal(new DateTime(2024, 1, 14).AddTicks(-1), year.End);
    }


    [Fact]
    public void BiWeekPayrollPeriodUsTest()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar();
        var year = new BiWeekPayrollPeriod(culture, calendar, new DateTime(2023, 3, 1));
        Assert.Equal(new DateTime(2023, 2, 26), year.Start);
        Assert.Equal(new DateTime(2023, 3, 12).AddTicks(-1), year.End);
    }

    [Fact]
    public void BiWeekPayrollPeriodYearEndTest()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar();
        var previousBiWeek = new BiWeekPayrollPeriod(culture, calendar, new DateTime(2023, 12, 1));
        Assert.Equal(new DateTime(2023, 11, 19), previousBiWeek.Start);
        Assert.Equal(new DateTime(2023, 12, 3).AddTicks(-1), previousBiWeek.End);

        var nextBiWeek = new BiWeekPayrollPeriod(culture, calendar, new DateTime(2024, 1, 1));
        Assert.Equal(new DateTime(2023, 12, 31), nextBiWeek.Start);
        Assert.Equal(new DateTime(2024, 1, 14).AddTicks(-1), nextBiWeek.End);
    }

    [Fact]
    public void BiWeekPayrollPeriodYearStartFebruaryTest()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar
        {
            FirstMonthOfYear = Month.January
        };

        var previousBiWeek = new BiWeekPayrollPeriod(culture, calendar, new DateTime(2023, 1, 1));
        Assert.Equal(new DateTime(2023, 1, 1), previousBiWeek.Start);
        Assert.Equal(new DateTime(2023, 1, 15).AddTicks(-1), previousBiWeek.End);

        var nextBiWeek = new BiWeekPayrollPeriod(culture, calendar, new DateTime(2023, 3, 1));
        Assert.Equal(new DateTime(2023, 2, 26), nextBiWeek.Start);
        Assert.Equal(new DateTime(2023, 3, 12).AddTicks(-1), nextBiWeek.End);
    }

    [Fact]
    public void BiWeekPayrollPeriodYearStartDecemberTest()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar
        {
            FirstMonthOfYear = Month.December
        };

        var previousBiWeek = new BiWeekPayrollPeriod(culture, calendar, new DateTime(2022, 11, 1));
        Assert.Equal(new DateTime(2022, 10, 30), previousBiWeek.Start);
        Assert.Equal(new DateTime(2022, 11, 13).AddTicks(-1), previousBiWeek.End);

        var nextBiWeek = new BiWeekPayrollPeriod(culture, calendar, new DateTime(2022, 12, 1));
        Assert.Equal(new DateTime(2022, 11, 27), nextBiWeek.Start);
        Assert.Equal(new DateTime(2022, 12, 11).AddTicks(-1), nextBiWeek.End);
    }
}