using System;
using System.Globalization;
using Xunit;

namespace PayrollEngine.Domain.Model.Tests;

public class QuarterPayrollPeriodTests
{
    [Fact]
    public void QuarterPayrollPeriodUsTest()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar();
        var year = new QuarterPayrollPeriod(culture, calendar, new DateTime(2023, 3, 1));
        Assert.Equal(new DateTime(2023, 1, 1), year.Start);
        Assert.Equal(new DateTime(2023, 4, 1).AddTicks(-1), year.End);
    }

    [Fact]
    public void QuarterPayrollPeriodYearEndTest()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar();
        var previousQuarter = new QuarterPayrollPeriod(culture, calendar, new DateTime(2023, 12, 1));
        Assert.Equal(new DateTime(2023, 10, 1), previousQuarter.Start);
        Assert.Equal(new DateTime(2024, 1, 1).AddTicks(-1), previousQuarter.End);

        var nextQuarter = new QuarterPayrollPeriod(culture, calendar, new DateTime(2024, 1, 1));
        Assert.Equal(new DateTime(2024, 1, 1), nextQuarter.Start);
        Assert.Equal(new DateTime(2024, 4, 1).AddTicks(-1), nextQuarter.End);
    }

    [Fact]
    public void QuarterPayrollPeriodYearStartFebruaryTest()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar
        {
            FirstMonthOfYear = Month.February
        };

        var previousQuarter = new QuarterPayrollPeriod(culture, calendar, new DateTime(2023, 1, 1));
        Assert.Equal(new DateTime(2022, 11, 1), previousQuarter.Start);
        Assert.Equal(new DateTime(2023, 2, 1).AddTicks(-1), previousQuarter.End);

        var nextQuarter = new QuarterPayrollPeriod(culture, calendar, new DateTime(2023, 3, 1));
        Assert.Equal(new DateTime(2023, 2, 1), nextQuarter.Start);
        Assert.Equal(new DateTime(2023, 5, 1).AddTicks(-1), nextQuarter.End);
    }
        
    [Fact]
    public void QuarterPayrollPeriodYearStartDecemberTest()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar
        {
            FirstMonthOfYear = Month.December
        };

        var previousQuarter = new QuarterPayrollPeriod(culture, calendar, new DateTime(2022, 11, 1));
        Assert.Equal(new DateTime(2022, 9, 1), previousQuarter.Start);
        Assert.Equal(new DateTime(2022, 12, 1).AddTicks(-1), previousQuarter.End);

        var nextQuarter = new QuarterPayrollPeriod(culture, calendar, new DateTime(2022, 12, 1));
        Assert.Equal(new DateTime(2022, 12, 1), nextQuarter.Start);
        Assert.Equal(new DateTime(2023, 3, 1).AddTicks(-1), nextQuarter.End);
    }

    [Fact]
    public void QuarterPayrollPeriodYearStartAprilTest()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar
        {
            FirstMonthOfYear = Month.April
        };

        var previousQuarter = new QuarterPayrollPeriod(culture, calendar, new DateTime(2023, 3, 1));
        Assert.Equal(new DateTime(2023, 1, 1), previousQuarter.Start);
        Assert.Equal(new DateTime(2023, 4, 1).AddTicks(-1), previousQuarter.End);

        var nextQuarter = new QuarterPayrollPeriod(culture, calendar, new DateTime(2023, 4, 1));
        Assert.Equal(new DateTime(2023, 4, 1), nextQuarter.Start);
        Assert.Equal(new DateTime(2023, 7, 1).AddTicks(-1), nextQuarter.End);
    }

    [Fact]
    public void QuarterPayrollPeriodYearStartJuly1Test()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar
        {
            FirstMonthOfYear = Month.July
        };

        var previousQuarter = new QuarterPayrollPeriod(culture, calendar, new DateTime(2023, 2, 1));
        Assert.Equal(new DateTime(2023, 1, 1), previousQuarter.Start);
        Assert.Equal(new DateTime(2023, 4, 1).AddTicks(-1), previousQuarter.End);

        var nextQuarter = new QuarterPayrollPeriod(culture, calendar, new DateTime(2023, 11, 1));
        Assert.Equal(new DateTime(2023, 10, 1), nextQuarter.Start);
        Assert.Equal(new DateTime(2024, 1, 1).AddTicks(-1), nextQuarter.End);
    }

    [Fact]
    public void QuarterPayrollPeriodYearStartJuly2Test()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar
        {
            FirstMonthOfYear = Month.July
        };

        var previousQuarter = new QuarterPayrollPeriod(culture, calendar, new DateTime(2023, 6, 1));
        Assert.Equal(new DateTime(2023, 4, 1), previousQuarter.Start);
        Assert.Equal(new DateTime(2023, 7, 1).AddTicks(-1), previousQuarter.End);

        var nextQuarter = new QuarterPayrollPeriod(culture, calendar, new DateTime(2023, 7, 1));
        Assert.Equal(new DateTime(2023, 7, 1), nextQuarter.Start);
        Assert.Equal(new DateTime(2023, 10, 1).AddTicks(-1), nextQuarter.End);
    }

    [Fact]
    public void QuarterPayrollPeriodYearStartOctoberTest()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar
        {
            FirstMonthOfYear = Month.October
        };

        var previousQuarter = new QuarterPayrollPeriod(culture, calendar, new DateTime(2023, 8, 1));
        Assert.Equal(new DateTime(2023, 7, 1), previousQuarter.Start);
        Assert.Equal(new DateTime(2023, 10, 1).AddTicks(-1), previousQuarter.End);

        var nextQuarter = new QuarterPayrollPeriod(culture, calendar, new DateTime(2023, 11, 1));
        Assert.Equal(new DateTime(2023, 10, 1), nextQuarter.Start);
        Assert.Equal(new DateTime(2024, 1, 1).AddTicks(-1), nextQuarter.End);
    }
}