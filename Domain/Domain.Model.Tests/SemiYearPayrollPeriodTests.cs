using System;
using System.Globalization;
using Xunit;

namespace PayrollEngine.Domain.Model.Tests
{
    public class SemiYearPayrollPeriodTests
    {
        [Fact]
        public void SemiYearPayrollPeriodUsTest()
        {
            var culture = new CultureInfo("en-US");

            var calendar = new Calendar();
            var year = new SemiYearPayrollPeriod(culture, calendar, new DateTime(2023, 3, 1));
            Assert.Equal(new DateTime(2023, 1, 1), year.Start);
            Assert.Equal(new DateTime(2023, 7, 1).AddTicks(-1), year.End);
        }

        [Fact]
        public void SemiYearPayrollPeriodYearEndTest()
        {
            var culture = new CultureInfo("en-US");

            var calendar = new Calendar();
            var previousSemiYear = new SemiYearPayrollPeriod(culture, calendar, new DateTime(2023, 12, 1));
            Assert.Equal(new DateTime(2023, 7, 1), previousSemiYear.Start);
            Assert.Equal(new DateTime(2024, 1, 1).AddTicks(-1), previousSemiYear.End);

            var nextSemiYear = new SemiYearPayrollPeriod(culture, calendar, new DateTime(2024, 1, 1));
            Assert.Equal(new DateTime(2024, 1, 1), nextSemiYear.Start);
            Assert.Equal(new DateTime(2024, 7, 1).AddTicks(-1), nextSemiYear.End);
        }

        [Fact]
        public void SemiYearPayrollPeriodYearStartFebruaryTest()
        {
            var culture = new CultureInfo("en-US");

            var calendar = new Calendar
            {
                FirstMonthOfYear = Month.February
            };

            var previousSemiYear = new SemiYearPayrollPeriod(culture, calendar, new DateTime(2023, 1, 1));
            Assert.Equal(new DateTime(2022, 8, 1), previousSemiYear.Start);
            Assert.Equal(new DateTime(2023, 2, 1).AddTicks(-1), previousSemiYear.End);

            var nextSemiYear = new SemiYearPayrollPeriod(culture, calendar, new DateTime(2023, 3, 1));
            Assert.Equal(new DateTime(2023, 2, 1), nextSemiYear.Start);
            Assert.Equal(new DateTime(2023, 8, 1).AddTicks(-1), nextSemiYear.End);
        }
        
        [Fact]
        public void SemiYearPayrollPeriodYearStartDecemberTest()
        {
            var culture = new CultureInfo("en-US");

            var calendar = new Calendar
            {
                FirstMonthOfYear = Month.December
            };

            var previousSemiYear = new SemiYearPayrollPeriod(culture, calendar, new DateTime(2022, 11, 1));
            Assert.Equal(new DateTime(2022, 6, 1), previousSemiYear.Start);
            Assert.Equal(new DateTime(2022, 12, 1).AddTicks(-1), previousSemiYear.End);

            var nextSemiYear = new SemiYearPayrollPeriod(culture, calendar, new DateTime(2022, 12, 1));
            Assert.Equal(new DateTime(2022, 12, 1), nextSemiYear.Start);
            Assert.Equal(new DateTime(2023, 6, 1).AddTicks(-1), nextSemiYear.End);
        }

        [Fact]
        public void SemiYearPayrollPeriodYearStartAprilTest()
        {
            var culture = new CultureInfo("en-US");

            var calendar = new Calendar
            {
                FirstMonthOfYear = Month.April
            };

            var previousSemiYear = new SemiYearPayrollPeriod(culture, calendar, new DateTime(2023, 3, 1));
            Assert.Equal(new DateTime(2022, 10, 1), previousSemiYear.Start);
            Assert.Equal(new DateTime(2023, 4, 1).AddTicks(-1), previousSemiYear.End);

            var nextSemiYear = new SemiYearPayrollPeriod(culture, calendar, new DateTime(2023, 4, 1));
            Assert.Equal(new DateTime(2023, 4, 1), nextSemiYear.Start);
            Assert.Equal(new DateTime(2023, 10, 1).AddTicks(-1), nextSemiYear.End);
        }

        [Fact]
        public void SemiYearPayrollPeriodYearStartJulyTest()
        {
            var culture = new CultureInfo("en-US");

            var calendar = new Calendar
            {
                FirstMonthOfYear = Month.July
            };

            var previousSemiYear = new SemiYearPayrollPeriod(culture, calendar, new DateTime(2023, 3, 1));
            Assert.Equal(new DateTime(2023, 1, 1), previousSemiYear.Start);
            Assert.Equal(new DateTime(2023, 7, 1).AddTicks(-1), previousSemiYear.End);

            var nextSemiYear = new SemiYearPayrollPeriod(culture, calendar, new DateTime(2023, 9, 1));
            Assert.Equal(new DateTime(2023, 7, 1), nextSemiYear.Start);
            Assert.Equal(new DateTime(2024, 1, 1).AddTicks(-1), nextSemiYear.End);
        }

        [Fact]
        public void SemiYearPayrollPeriodYearStartOctoberTest()
        {
            var culture = new CultureInfo("en-US");

            var calendar = new Calendar
            {
                FirstMonthOfYear = Month.October
            };

            var previousSemiYear = new SemiYearPayrollPeriod(culture, calendar, new DateTime(2023, 8, 1));
            Assert.Equal(new DateTime(2023, 4, 1), previousSemiYear.Start);
            Assert.Equal(new DateTime(2023, 10, 1).AddTicks(-1), previousSemiYear.End);

            var nextSemiYear = new SemiYearPayrollPeriod(culture, calendar, new DateTime(2023, 11, 1));
            Assert.Equal(new DateTime(2023, 10, 1), nextSemiYear.Start);
            Assert.Equal(new DateTime(2024, 4, 1).AddTicks(-1), nextSemiYear.End);
        }
    }
}
