using System;
using System.Globalization;
using Xunit;

namespace PayrollEngine.Domain.Model.Tests;

public class BiMonthPayrollPeriodTests
{
    [Fact]
    public void BiMonthPayrollPeriodUsTest()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar();
        var year = new BiMonthPayrollPeriod(culture, calendar, new DateTime(2023, 3, 1));
        Assert.Equal(new DateTime(2023, 3, 1), year.Start);
        Assert.Equal(new DateTime(2023, 5, 1).AddTicks(-1), year.End);
    }

    [Fact]
    public void BiMonthPayrollPeriodYearEndTest()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar();
        var previousBiMonth = new BiMonthPayrollPeriod(culture, calendar, new DateTime(2023, 12, 1));
        Assert.Equal(new DateTime(2023, 11, 1), previousBiMonth.Start);
        Assert.Equal(new DateTime(2024, 1, 1).AddTicks(-1), previousBiMonth.End);

        var nextBiMonth = new BiMonthPayrollPeriod(culture, calendar, new DateTime(2024, 1, 1));
        Assert.Equal(new DateTime(2024, 1, 1), nextBiMonth.Start);
        Assert.Equal(new DateTime(2024, 3, 1).AddTicks(-1), nextBiMonth.End);
    }

    [Fact]
    public void BiMonthPayrollPeriodYearStartFebruaryTest()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar
        {
            FirstMonthOfYear = Month.February
        };

        var previousBiMonth = new BiMonthPayrollPeriod(culture, calendar, new DateTime(2023, 1, 1));
        Assert.Equal(new DateTime(2022, 12, 1), previousBiMonth.Start);
        Assert.Equal(new DateTime(2023, 2, 1).AddTicks(-1), previousBiMonth.End);

        var nextBiMonth = new BiMonthPayrollPeriod(culture, calendar, new DateTime(2023, 3, 1));
        Assert.Equal(new DateTime(2023, 2, 1), nextBiMonth.Start);
        Assert.Equal(new DateTime(2023, 4, 1).AddTicks(-1), nextBiMonth.End);
    }
        
    [Fact]
    public void BiMonthPayrollPeriodYearStartDecemberTest()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar
        {
            FirstMonthOfYear = Month.December
        };

        var previousBiMonth = new BiMonthPayrollPeriod(culture, calendar, new DateTime(2022, 11, 1));
        Assert.Equal(new DateTime(2022, 10, 1), previousBiMonth.Start);
        Assert.Equal(new DateTime(2022, 12, 1).AddTicks(-1), previousBiMonth.End);

        var nextBiMonth = new BiMonthPayrollPeriod(culture, calendar, new DateTime(2022, 12, 1));
        Assert.Equal(new DateTime(2022, 12, 1), nextBiMonth.Start);
        Assert.Equal(new DateTime(2023, 2, 1).AddTicks(-1), nextBiMonth.End);
    }

    [Fact]
    public void BiMonthPayrollPeriodYearStartAprilTest()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar
        {
            FirstMonthOfYear = Month.April
        };

        var previousBiMonth = new BiMonthPayrollPeriod(culture, calendar, new DateTime(2023, 3, 1));
        Assert.Equal(new DateTime(2023, 2, 1), previousBiMonth.Start);
        Assert.Equal(new DateTime(2023, 4, 1).AddTicks(-1), previousBiMonth.End);

        var nextBiMonth = new BiMonthPayrollPeriod(culture, calendar, new DateTime(2023, 4, 1));
        Assert.Equal(new DateTime(2023, 4, 1), nextBiMonth.Start);
        Assert.Equal(new DateTime(2023, 6, 1).AddTicks(-1), nextBiMonth.End);
    }

    [Fact]
    public void BiMonthPayrollPeriodYearStartJuly1Test()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar
        {
            FirstMonthOfYear = Month.July
        };

        var previousBiMonth = new BiMonthPayrollPeriod(culture, calendar, new DateTime(2023, 2, 1));
        Assert.Equal(new DateTime(2023, 1, 1), previousBiMonth.Start);
        Assert.Equal(new DateTime(2023, 3, 1).AddTicks(-1), previousBiMonth.End);

        var nextBiMonth = new BiMonthPayrollPeriod(culture, calendar, new DateTime(2023, 11, 1));
        Assert.Equal(new DateTime(2023, 11, 1), nextBiMonth.Start);
        Assert.Equal(new DateTime(2024, 1, 1).AddTicks(-1), nextBiMonth.End);
    }

    [Fact]
    public void BiMonthPayrollPeriodYearStartJuly2Test()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar
        {
            FirstMonthOfYear = Month.July
        };

        var previousBiMonth = new BiMonthPayrollPeriod(culture, calendar, new DateTime(2023, 6, 1));
        Assert.Equal(new DateTime(2023, 5, 1), previousBiMonth.Start);
        Assert.Equal(new DateTime(2023, 7, 1).AddTicks(-1), previousBiMonth.End);

        var nextBiMonth = new BiMonthPayrollPeriod(culture, calendar, new DateTime(2023, 7, 1));
        Assert.Equal(new DateTime(2023, 7, 1), nextBiMonth.Start);
        Assert.Equal(new DateTime(2023, 9, 1).AddTicks(-1), nextBiMonth.End);
    }

    [Fact]
    public void BiMonthPayrollPeriodYearStartOctoberTest()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar
        {
            FirstMonthOfYear = Month.October
        };

        var previousBiMonth = new BiMonthPayrollPeriod(culture, calendar, new DateTime(2023, 8, 1));
        Assert.Equal(new DateTime(2023, 8, 1), previousBiMonth.Start);
        Assert.Equal(new DateTime(2023, 10, 1).AddTicks(-1), previousBiMonth.End);

        var nextBiMonth = new BiMonthPayrollPeriod(culture, calendar, new DateTime(2023, 11, 1));
        Assert.Equal(new DateTime(2023, 10, 1), nextBiMonth.Start);
        Assert.Equal(new DateTime(2023, 12, 1).AddTicks(-1), nextBiMonth.End);
    }
}