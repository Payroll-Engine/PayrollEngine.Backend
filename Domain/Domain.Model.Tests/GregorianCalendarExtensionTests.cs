using System;
using System.Globalization;
using Xunit;

namespace PayrollEngine.Domain.Model.Tests
{
    public class GregorianCalendarExtensionTests
    {
        [Fact]
        public void GetOffsetDateYearSwissTest()
        {
            var calendar = new GregorianCalendar();

            var unchanged = calendar.GetOffsetDate(2023, 3);
            Assert.Equal(new DateTime(2023, 3, 1), unchanged);

            var nextYear = calendar.GetOffsetDate(2024, 3);
            Assert.Equal(new DateTime(2024, 3, 1), nextYear);

            var prevYear = calendar.GetOffsetDate(2022, 3);
            Assert.Equal(new DateTime(2022, 3, 1), prevYear);

            var nextTenYear = calendar.GetOffsetDate(2033, 3);
            Assert.Equal(new DateTime(2033, 3, 1), nextTenYear);

            var prevTenYear = calendar.GetOffsetDate(2013, 3);
            Assert.Equal(new DateTime(2013, 3, 1), prevTenYear);

        }

        [Fact]
        public void GetOffsetDateMonthSwissTest()
        {
            var calendar = new GregorianCalendar();

            var unchanged = calendar.GetOffsetDate(2023, 3, offsetMonths: 0);
            Assert.Equal(new DateTime(2023, 3, 1), unchanged);

            var nextMonth = calendar.GetOffsetDate(2023, 3, offsetMonths: 1);
            Assert.Equal(new DateTime(2023, 4, 1), nextMonth);

            var prevMonth = calendar.GetOffsetDate(2023, 3, offsetMonths: -1);
            Assert.Equal(new DateTime(2023, 2, 1), prevMonth);

            var next12Months = calendar.GetOffsetDate(2023, 3, offsetMonths: 12);
            Assert.Equal(new DateTime(2024, 3, 1), next12Months);

            var prev12Months = calendar.GetOffsetDate(2023, 3, offsetMonths: -12);
            Assert.Equal(new DateTime(2022, 3, 1), prev12Months);

            var next21Months = calendar.GetOffsetDate(2023, 3, offsetMonths: 21);
            Assert.Equal(new DateTime(2024, 12, 1), next21Months);

            var prev21Months = calendar.GetOffsetDate(2023, 3, offsetMonths: -21);
            Assert.Equal(new DateTime(2021, 6, 1), prev21Months);

            var next22Months = calendar.GetOffsetDate(2023, 3, offsetMonths: 22);
            Assert.Equal(new DateTime(2025, 1, 1), next22Months);

            var prev22Months = calendar.GetOffsetDate(2023, 3, offsetMonths: -22);
            Assert.Equal(new DateTime(2021, 5, 1), prev22Months);

            var next26Months = calendar.GetOffsetDate(2023, 3, offsetMonths: 26);
            Assert.Equal(new DateTime(2025, 5, 1), next26Months);

            var prev26Months = calendar.GetOffsetDate(2023, 3, offsetMonths: -26);
            Assert.Equal(new DateTime(2021, 1, 1), prev26Months);

            var next27Months = calendar.GetOffsetDate(2023, 3, offsetMonths: 27);
            Assert.Equal(new DateTime(2025, 6, 1), next27Months);

            var prev27Months = calendar.GetOffsetDate(2023, 3, offsetMonths: -27);
            Assert.Equal(new DateTime(2020, 12, 1), prev27Months);
        }


        [Fact]
        public void GetOffsetDateYearMonthSwissTest()
        {
            var calendar = new GregorianCalendar();

            var unchanged = calendar.GetOffsetDate(2023, 3, offsetMonths: 0);
            Assert.Equal(new DateTime(2023, 3, 1), unchanged);

            var nextMonth = calendar.GetOffsetDate(2024, 3, offsetMonths: 1);
            Assert.Equal(new DateTime(2024, 4, 1), nextMonth);

            var nextYearMonth = calendar.GetOffsetDate(2024, 3, offsetMonths: -1);
            Assert.Equal(new DateTime(2024, 2, 1), nextYearMonth);

            var prevYearMonth = calendar.GetOffsetDate(2022, 3, offsetMonths: -1);
            Assert.Equal(new DateTime(2022, 2, 1), prevYearMonth);

        }
    }
}
