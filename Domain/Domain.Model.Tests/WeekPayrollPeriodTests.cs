using System;
using System.Globalization;
using Xunit;

namespace PayrollEngine.Domain.Model.Tests
{
    public class WeekPayrollPeriodTests
    {
        [Fact]
        public void WeekPayrollYearFirst1Test()
        {
            var culture = new CultureInfo("en-US");

            var calendar = new Calendar();
            var year = new WeekPayrollPeriod(culture, calendar, new DateTime(2023, 1, 1));
            Assert.Equal(new DateTime(2023, 1, 1), year.Start);
            Assert.Equal(new DateTime(2023, 1, 8).AddTicks(-1), year.End);
        }

        [Fact]
        public void WeekPayrollYearFirst2Test()
        {
            var culture = new CultureInfo("en-US");

            var calendar = new Calendar();
            var year = new WeekPayrollPeriod(culture, calendar, new DateTime(2023, 1, 11));
            Assert.Equal(new DateTime(2023, 1, 8), year.Start);
            Assert.Equal(new DateTime(2023, 1, 15).AddTicks(-1), year.End);
        }

        [Fact]
        public void WeekPayrollYearSecondTest()
        {
            var culture = new CultureInfo("en-US");

            var calendar = new Calendar();
            var year = new WeekPayrollPeriod(culture, calendar, new DateTime(2023, 1, 18));
            Assert.Equal(new DateTime(2023, 1, 15), year.Start);
            Assert.Equal(new DateTime(2023, 1, 22).AddTicks(-1), year.End);
        }

        [Fact]
        public void WeekPayrollYearSecondLastTest()
        {
            var culture = new CultureInfo("en-US");

            var calendar = new Calendar();
            var year = new WeekPayrollPeriod(culture, calendar, new DateTime(2023, 12, 28));
            Assert.Equal(new DateTime(2023, 12, 24), year.Start);
            Assert.Equal(new DateTime(2023, 12, 31).AddTicks(-1), year.End);
        }

        [Fact]
        public void WeekPayrollYearLastTest()
        {
            var culture = new CultureInfo("en-US");

            var calendar = new Calendar();
            var year = new WeekPayrollPeriod(culture, calendar, new DateTime(2023, 12, 31));
            Assert.Equal(new DateTime(2023, 12, 31), year.Start);
            Assert.Equal(new DateTime(2024, 1, 7).AddTicks(-1), year.End);
        }

        [Fact]
        public void WeekPayrollPeriodUsTest()
        {
            var culture = new CultureInfo("en-US");

            var calendar = new Calendar();
            var year = new WeekPayrollPeriod(culture, calendar, new DateTime(2023, 3, 1));
            Assert.Equal(new DateTime(2023, 2, 26), year.Start);
            Assert.Equal(new DateTime(2023, 3, 5).AddTicks(-1), year.End);
        }

        [Fact]
        public void WeekPayrollPeriodYearEndTest()
        {
            var culture = new CultureInfo("en-US");

            var calendar = new Calendar();
            var previousWeek = new WeekPayrollPeriod(culture, calendar, new DateTime(2023, 12, 1));
            Assert.Equal(new DateTime(2023, 11, 26), previousWeek.Start);
            Assert.Equal(new DateTime(2023, 12, 3).AddTicks(-1), previousWeek.End);

            var nextWeek = new WeekPayrollPeriod(culture, calendar, new DateTime(2024, 1, 1));
            Assert.Equal(new DateTime(2023, 12, 31), nextWeek.Start);
            Assert.Equal(new DateTime(2024, 1, 7).AddTicks(-1), nextWeek.End);
        }

        [Fact]
        public void WeekPayrollPeriodYearStartFebruaryTest()
        {
            var culture = new CultureInfo("en-US");

            var calendar = new Calendar
            {
                FirstMonthOfYear = Month.January
            };

            var previousWeek = new WeekPayrollPeriod(culture, calendar, new DateTime(2023, 1, 1));
            Assert.Equal(new DateTime(2023, 1, 1), previousWeek.Start);
            Assert.Equal(new DateTime(2023, 1, 8).AddTicks(-1), previousWeek.End);

            var nextWeek = new WeekPayrollPeriod(culture, calendar, new DateTime(2023, 3, 1));
            Assert.Equal(new DateTime(2023, 2, 26), nextWeek.Start);
            Assert.Equal(new DateTime(2023, 3, 5).AddTicks(-1), nextWeek.End);
        }

        [Fact]
        public void WeekPayrollPeriodYearStartDecemberTest()
        {
            var culture = new CultureInfo("en-US");

            var calendar = new Calendar
            {
                FirstMonthOfYear = Month.December
            };

            var previousWeek = new WeekPayrollPeriod(culture, calendar, new DateTime(2022, 11, 1));
            Assert.Equal(new DateTime(2022, 10, 30), previousWeek.Start);
            Assert.Equal(new DateTime(2022, 11, 6).AddTicks(-1), previousWeek.End);

            var nextWeek = new WeekPayrollPeriod(culture, calendar, new DateTime(2022, 12, 1));
            Assert.Equal(new DateTime(2022, 11, 27), nextWeek.Start);
            Assert.Equal(new DateTime(2022, 12, 4).AddTicks(-1), nextWeek.End);
        }
    }
}
