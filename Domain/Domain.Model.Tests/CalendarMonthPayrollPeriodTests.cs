using System;
using System.Globalization;
using Xunit;

namespace PayrollEngine.Domain.Model.Tests
{
    public class CalendarMonthPayrollPeriodTests
    {
        [Fact]
        public void CalendarMonthPayrollPeriodUsTest()
        {
            var culture = new CultureInfo("en-US");

            var calendar = new Calendar();
            var year = new CalendarMonthPayrollPeriod(culture, calendar, new DateTime(2023, 3, 1));
            Assert.Equal(new DateTime(2023, 3, 1), year.Start);
            Assert.Equal(new DateTime(2023, 4, 1).AddTicks(-1), year.End);
        }

        [Fact]
        public void CalendarMonthPayrollPeriodYearEndTest()
        {
            var culture = new CultureInfo("en-US");

            var calendar = new Calendar();
            var previousCalendarMonth = new CalendarMonthPayrollPeriod(culture, calendar, new DateTime(2023, 12, 1));
            Assert.Equal(new DateTime(2023, 12, 1), previousCalendarMonth.Start);
            Assert.Equal(new DateTime(2024, 1, 1).AddTicks(-1), previousCalendarMonth.End);

            var nextCalendarMonth = new CalendarMonthPayrollPeriod(culture, calendar, new DateTime(2024, 1, 1));
            Assert.Equal(new DateTime(2024, 1, 1), nextCalendarMonth.Start);
            Assert.Equal(new DateTime(2024, 2, 1).AddTicks(-1), nextCalendarMonth.End);
        }

        [Fact]
        public void CalendarMonthPayrollPeriodYearStartFebruaryTest()
        {
            var culture = new CultureInfo("en-US");

            var calendar = new Calendar
            {
                FirstMonthOfYear = Month.January
            };

            var previousCalendarMonth = new CalendarMonthPayrollPeriod(culture, calendar, new DateTime(2023, 1, 1));
            Assert.Equal(new DateTime(2023, 1, 1), previousCalendarMonth.Start);
            Assert.Equal(new DateTime(2023, 2, 1).AddTicks(-1), previousCalendarMonth.End);

            var nextCalendarMonth = new CalendarMonthPayrollPeriod(culture, calendar, new DateTime(2023, 3, 1));
            Assert.Equal(new DateTime(2023, 3, 1), nextCalendarMonth.Start);
            Assert.Equal(new DateTime(2023, 4, 1).AddTicks(-1), nextCalendarMonth.End);
        }

        [Fact]
        public void CalendarMonthPayrollPeriodYearStartDecemberTest()
        {
            var culture = new CultureInfo("en-US");

            var calendar = new Calendar
            {
                FirstMonthOfYear = Month.December
            };

            var previousCalendarMonth = new CalendarMonthPayrollPeriod(culture, calendar, new DateTime(2022, 11, 1));
            Assert.Equal(new DateTime(2022, 11, 1), previousCalendarMonth.Start);
            Assert.Equal(new DateTime(2022, 12, 1).AddTicks(-1), previousCalendarMonth.End);

            var nextCalendarMonth = new CalendarMonthPayrollPeriod(culture, calendar, new DateTime(2022, 12, 1));
            Assert.Equal(new DateTime(2022, 12, 1), nextCalendarMonth.Start);
            Assert.Equal(new DateTime(2023, 1, 1).AddTicks(-1), nextCalendarMonth.End);
        }
    }
}
