using System;
using System.Globalization;
using Xunit;

namespace PayrollEngine.Domain.Model.Tests;

public class SemiMonthPayrollPeriodTests
{
    [Fact]
    public void SemiMonthPayrollFirstPeriodStartTest()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar();
        var year = new SemiMonthPayrollPeriod(culture, calendar, new DateTime(2023, 1, 1));
        Assert.Equal(new DateTime(2023, 1, 1), year.Start);
        Assert.Equal(new DateTime(2023, 1, 16).AddTicks(-1), year.End);
    }

    [Fact]
    public void SemiMonthPayrollFirstPeriodEndTest()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar();
        var year = new SemiMonthPayrollPeriod(culture, calendar, new DateTime(2023, 1, 15));
        Assert.Equal(new DateTime(2023, 1, 1), year.Start);
        Assert.Equal(new DateTime(2023, 1, 16).AddTicks(-1), year.End);
    }

    [Fact]
    public void SemiMonthPayrollSecondPeriodStartTest()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar();
        var year = new SemiMonthPayrollPeriod(culture, calendar, new DateTime(2023, 1, 16));
        Assert.Equal(new DateTime(2023, 1, 16), year.Start);
        Assert.Equal(new DateTime(2023, 2, 1).AddTicks(-1), year.End);
    }

    [Fact]
    public void SemiMonthPayrollSecondPeriodEndTest()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar();
        var year = new SemiMonthPayrollPeriod(culture, calendar, new DateTime(2023, 1, 31));
        Assert.Equal(new DateTime(2023, 1, 16), year.Start);
        Assert.Equal(new DateTime(2023, 2, 1).AddTicks(-1), year.End);
    }

    [Fact]
    public void SemiMonthPayrollFebruaryFirstPeriodStartTest()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar();
        var year = new SemiMonthPayrollPeriod(culture, calendar, new DateTime(2023, 2, 1));
        Assert.Equal(new DateTime(2023, 2, 1), year.Start);
        Assert.Equal(new DateTime(2023, 2, 16).AddTicks(-1), year.End);
    }

    [Fact]
    public void SemiMonthPayrollFebruaryFirstPeriodEndTest()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar();
        var year = new SemiMonthPayrollPeriod(culture, calendar, new DateTime(2023, 2, 15));
        Assert.Equal(new DateTime(2023, 2, 1), year.Start);
        Assert.Equal(new DateTime(2023, 2, 16).AddTicks(-1), year.End);
    }

    [Fact]
    public void SemiMonthPayrollFebruarySecondPeriodStartTest()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar();
        var year = new SemiMonthPayrollPeriod(culture, calendar, new DateTime(2023, 2, 16));
        Assert.Equal(new DateTime(2023, 2, 16), year.Start);
        Assert.Equal(new DateTime(2023, 3, 1).AddTicks(-1), year.End);
    }

    [Fact]
    public void SemiMonthPayrollFebruarySecondPeriodEndTest()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar();
        var year = new SemiMonthPayrollPeriod(culture, calendar, new DateTime(2023, 2, 28));
        Assert.Equal(new DateTime(2023, 2, 16), year.Start);
        Assert.Equal(new DateTime(2023, 3, 1).AddTicks(-1), year.End);
    }


    [Fact]
    public void SemiMonthPayrollLeapFebruaryFirstPeriodStartTest()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar();
        var year = new SemiMonthPayrollPeriod(culture, calendar, new DateTime(2024, 2, 1));
        Assert.Equal(new DateTime(2024, 2, 1), year.Start);
        Assert.Equal(new DateTime(2024, 2, 16).AddTicks(-1), year.End);
    }

    [Fact]
    public void SemiMonthPayrollLeapFebruaryFirstPeriodEndTest()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar();
        var year = new SemiMonthPayrollPeriod(culture, calendar, new DateTime(2024, 2, 15));
        Assert.Equal(new DateTime(2024, 2, 1), year.Start);
        Assert.Equal(new DateTime(2024, 2, 16).AddTicks(-1), year.End);
    }

    [Fact]
    public void SemiMonthPayrollLeapFebruarySecondPeriodStartTest()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar();
        var year = new SemiMonthPayrollPeriod(culture, calendar, new DateTime(2024, 2, 16));
        Assert.Equal(new DateTime(2024, 2, 16), year.Start);
        Assert.Equal(new DateTime(2024, 3, 1).AddTicks(-1), year.End);
    }

    [Fact]
    public void SemiMonthPayrollLeapFebruarySecondPeriodEndTest()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar();
        var year = new SemiMonthPayrollPeriod(culture, calendar, new DateTime(2024, 2, 29));
        Assert.Equal(new DateTime(2024, 2, 16), year.Start);
        Assert.Equal(new DateTime(2024, 3, 1).AddTicks(-1), year.End);
    }

}