using System;
using System.Globalization;
using Xunit;

namespace PayrollEngine.Domain.Model.Tests;

public class LunisoralMonthPayrollPeriodTests
{
    [Fact]
    public void LunisoralMonthPayrollFirst1Test()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar();
        var month = new LunisoralMonthPayrollPeriod(culture, calendar, new DateTime(2023, 1, 1));
        Assert.Equal(new DateTime(2023, 1, 1), month.Start);
        Assert.Equal(new DateTime(2023, 1, 29).AddTicks(-1), month.End);
    }

    [Fact]
    public void LunisoralMonthPayrollFirst2Test()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar();
        var month = new LunisoralMonthPayrollPeriod(culture, calendar, new DateTime(2023, 1, 28));
        Assert.Equal(new DateTime(2023, 1, 1), month.Start);
        Assert.Equal(new DateTime(2023, 1, 29).AddTicks(-1), month.End);
    }

    [Fact]
    public void LunisoralMonthPayrollSecondTest()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar();
        var month = new LunisoralMonthPayrollPeriod(culture, calendar, new DateTime(2023, 1, 29));
        Assert.Equal(new DateTime(2023, 1, 29), month.Start);
        Assert.Equal(new DateTime(2023, 2, 26).AddTicks(-1), month.End);
    }

    [Fact]
    public void LunisoralMonthPayrollThirdTest()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar();
        var month = new LunisoralMonthPayrollPeriod(culture, calendar, new DateTime(2023, 2, 26));
        Assert.Equal(new DateTime(2023, 2, 26), month.Start);
        Assert.Equal(new DateTime(2023, 3, 26).AddTicks(-1), month.End);
    }

    [Fact]
    public void LunisoralMonthPayrollForthTest()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar();
        var month = new LunisoralMonthPayrollPeriod(culture, calendar, new DateTime(2023, 3, 26));
        Assert.Equal(new DateTime(2023, 3, 26), month.Start);
        Assert.Equal(new DateTime(2023, 4, 23).AddTicks(-1), month.End);
    }

    [Fact]
    public void LunisoralMonthPayrollYearSecondLastTest()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar();
        var month = new LunisoralMonthPayrollPeriod(culture, calendar, new DateTime(2023, 12, 28));
        Assert.Equal(new DateTime(2023, 12, 03), month.Start);
        Assert.Equal(new DateTime(2023, 12, 31).AddTicks(-1), month.End);
    }

    [Fact]
    public void LunisoralMonthPayrollYearLastTest()
    {
        var culture = new CultureInfo("en-US");

        var calendar = new Calendar();
        var month = new LunisoralMonthPayrollPeriod(culture, calendar, new DateTime(2023, 12, 31));
        Assert.Equal(new DateTime(2023, 12, 31), month.Start);
        Assert.Equal(new DateTime(2024, 1, 28).AddTicks(-1), month.End);
    }
}