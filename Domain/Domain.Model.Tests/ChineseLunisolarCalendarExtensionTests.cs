using System;
using System.Globalization;
using Xunit;

namespace PayrollEngine.Domain.Model.Tests
{
    public class ChineseLunisolarCalendarExtensionTests
    {
        [Fact]
        public void GetOffsetDateYearChineseTest()
        {
            var calendar = new ChineseLunisolarCalendar();

            var unchanged = calendar.GetOffsetDate(2023, 3);
            Assert.Equal(new DateTime(2023, 3, 22), unchanged);

            var nextYear = calendar.GetOffsetDate(2024, 3);
            Assert.Equal(new DateTime(2024, 4, 9), nextYear);

            var prevYear = calendar.GetOffsetDate(2022, 3);
            Assert.Equal(new DateTime(2022, 4, 1), prevYear);

            var nextTenYear = calendar.GetOffsetDate(2033, 3);
            Assert.Equal(new DateTime(2033, 3, 31), nextTenYear);

            var prevTenYear = calendar.GetOffsetDate(2013, 3);
            Assert.Equal(new DateTime(2013, 4, 10), prevTenYear);

        }

        [Fact]
        public void GetOffsetDateMonthChineseTest()
        {
            var calendar = new ChineseLunisolarCalendar();

            var unchanged = calendar.GetOffsetDate(2023, 3, offsetMonths: 0);
            Assert.Equal(new DateTime(2023, 3, 22), unchanged);

            var nextMonth = calendar.GetOffsetDate(2023, 3, offsetMonths: 1);
            Assert.Equal(new DateTime(2023, 4, 20), nextMonth);

            var prevMonth = calendar.GetOffsetDate(2023, 3, offsetMonths: -1);
            Assert.Equal(new DateTime(2023, 2, 20), prevMonth);

            var next12Months = calendar.GetOffsetDate(2023, 3, offsetMonths: 12);
            Assert.Equal(new DateTime(2024, 4, 9), next12Months);

            var prev12Months = calendar.GetOffsetDate(2023, 3, offsetMonths: -12);
            Assert.Equal(new DateTime(2022, 4, 1), prev12Months);

            var next21Months = calendar.GetOffsetDate(2023, 3, offsetMonths: 21);
            Assert.Equal(new DateTime(2024, 12, 31), next21Months);

            var prev21Months = calendar.GetOffsetDate(2023, 3, offsetMonths: -21);
            Assert.Equal(new DateTime(2021, 7, 10), prev21Months);

            var next22Months = calendar.GetOffsetDate(2023, 3, offsetMonths: 22);
            Assert.Equal(new DateTime(2025, 1, 29), next22Months);

            var prev22Months = calendar.GetOffsetDate(2023, 3, offsetMonths: -22);
            Assert.Equal(new DateTime(2021, 6, 10), prev22Months);

            var next26Months = calendar.GetOffsetDate(2023, 3, offsetMonths: 26);
            Assert.Equal(new DateTime(2025, 5, 27), next26Months);

            var prev26Months = calendar.GetOffsetDate(2023, 3, offsetMonths: -26);
            Assert.Equal(new DateTime(2021, 2, 12), prev26Months);

            var next27Months = calendar.GetOffsetDate(2023, 3, offsetMonths: 27);
            Assert.Equal(new DateTime(2025, 6, 25), next27Months);

            var prev27Months = calendar.GetOffsetDate(2023, 3, offsetMonths: -27);
            Assert.Equal(new DateTime(2020, 12, 15), prev27Months);
        }

        [Fact]
        public void GetOffsetDateYearMonthChineseTest()
        {
            var calendar = new ChineseLunisolarCalendar();

            var unchanged = calendar.GetOffsetDate(2023, 3, offsetMonths: 0);
            Assert.Equal(new DateTime(2023, 3, 22), unchanged);

            var nextMonth = calendar.GetOffsetDate(2024, 3, offsetMonths: 1);
            Assert.Equal(new DateTime(2024, 5, 8), nextMonth);

            var nextYearMonth = calendar.GetOffsetDate(2024, 3, offsetMonths: -1);
            Assert.Equal(new DateTime(2024, 3, 10), nextYearMonth);

            var prevYearMonth = calendar.GetOffsetDate(2022, 3, offsetMonths: -1);
            Assert.Equal(new DateTime(2022, 3, 3), prevYearMonth);

        }
    }
}
