using System;
using System.Globalization;
using Xunit;

namespace PayrollEngine.Domain.Model.Tests
{
    public class YearPayrollPeriodTests
    {
        [Fact]
        public void YearPayrollPeriodUsTest()
        {
            var culture = new CultureInfo("en-US");

            var calendar = new Calendar();
            var year = new YearPayrollPeriod(culture, calendar, new DateTime(2023, 3, 1));
            Assert.Equal(new DateTime(2023, 1, 1), year.Start);
            Assert.Equal(new DateTime(2024, 1, 1).AddTicks(-1), year.End);
        }

        [Fact]
        public void YearPayrollPeriodYearEndTest()
        {
            var culture = new CultureInfo("en-US");

            var calendar = new Calendar();
            var previousYear = new YearPayrollPeriod(culture, calendar, new DateTime(2023, 12, 1));
            Assert.Equal(new DateTime(2023, 1, 1), previousYear.Start);
            Assert.Equal(new DateTime(2024, 1, 1).AddTicks(-1), previousYear.End);

            var nextYear = new YearPayrollPeriod(culture, calendar, new DateTime(2024, 11, 1));
            Assert.Equal(new DateTime(2024, 1, 1), nextYear.Start);
            Assert.Equal(new DateTime(2025, 1, 1).AddTicks(-1), nextYear.End);
        }

        [Fact]
        public void YearPayrollPeriodYearStartFebruaryTest()
        {
            var culture = new CultureInfo("en-US");

            var calendar = new Calendar
            {
                FirstMonthOfYear = Month.February
            };

            var previousYear = new YearPayrollPeriod(culture, calendar, new DateTime(2023, 1, 1));
            Assert.Equal(new DateTime(2022, 2, 1), previousYear.Start);
            Assert.Equal(new DateTime(2023, 2, 1).AddTicks(-1), previousYear.End);

            var nextYear = new YearPayrollPeriod(culture, calendar, new DateTime(2023, 3, 1));
            Assert.Equal(new DateTime(2023, 2, 1), nextYear.Start);
            Assert.Equal(new DateTime(2024, 2, 1).AddTicks(-1), nextYear.End);
        }
        
        [Fact]
        public void YearPayrollPeriodYearStartDecemberTest()
        {
            var culture = new CultureInfo("en-US");

            var calendar = new Calendar
            {
                FirstMonthOfYear = Month.December
            };

            var previousYear = new YearPayrollPeriod(culture, calendar, new DateTime(2022, 11, 1));
            Assert.Equal(new DateTime(2021, 12, 1), previousYear.Start);
            Assert.Equal(new DateTime(2022, 12, 1).AddTicks(-1), previousYear.End);

            var nextYear = new YearPayrollPeriod(culture, calendar, new DateTime(2022, 12, 1));
            Assert.Equal(new DateTime(2022, 12, 1), nextYear.Start);
            Assert.Equal(new DateTime(2023, 12, 1).AddTicks(-1), nextYear.End);
        }

        [Fact]
        public void YearPayrollPeriodYearStartAprilTest()
        {
            var culture = new CultureInfo("en-US");

            var calendar = new Calendar
            {
                FirstMonthOfYear = Month.April
            };

            var previousYear = new YearPayrollPeriod(culture, calendar, new DateTime(2023, 3, 1));
            Assert.Equal(new DateTime(2022, 4, 1), previousYear.Start);
            Assert.Equal(new DateTime(2023, 4, 1).AddTicks(-1), previousYear.End);

            var nextYear = new YearPayrollPeriod(culture, calendar, new DateTime(2023, 4, 1));
            Assert.Equal(new DateTime(2023, 4, 1), nextYear.Start);
            Assert.Equal(new DateTime(2024, 4, 1).AddTicks(-1), nextYear.End);
        }

        [Fact]
        public void YearPayrollPeriodYearStartJulyTest()
        {
            var culture = new CultureInfo("en-US");

            var calendar = new Calendar
            {
                FirstMonthOfYear = Month.July
            };

            var previousYear = new YearPayrollPeriod(culture, calendar, new DateTime(2023, 3, 1));
            Assert.Equal(new DateTime(2022, 7, 1), previousYear.Start);
            Assert.Equal(new DateTime(2023, 7, 1).AddTicks(-1), previousYear.End);

            var nextYear = new YearPayrollPeriod(culture, calendar, new DateTime(2023, 9, 1));
            Assert.Equal(new DateTime(2023, 7, 1), nextYear.Start);
            Assert.Equal(new DateTime(2024, 7, 1).AddTicks(-1), nextYear.End);
        }

        [Fact]
        public void YearPayrollPeriodYearStartOctoberTest()
        {
            var culture = new CultureInfo("en-US");

            var calendar = new Calendar
            {
                FirstMonthOfYear = Month.October
            };

            var previousYear = new YearPayrollPeriod(culture, calendar, new DateTime(2023, 8, 1));
            Assert.Equal(new DateTime(2022, 10, 1), previousYear.Start);
            Assert.Equal(new DateTime(2023, 10, 1).AddTicks(-1), previousYear.End);

            var nextYear = new YearPayrollPeriod(culture, calendar, new DateTime(2023, 11, 1));
            Assert.Equal(new DateTime(2023, 10, 1), nextYear.Start);
            Assert.Equal(new DateTime(2024, 10, 1).AddTicks(-1), nextYear.End);
        }
    }
}
