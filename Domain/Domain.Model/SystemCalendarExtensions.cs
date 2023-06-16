using System;

namespace PayrollEngine.Domain.Model
{
    /// <summary>
    /// Calendar Tool
    /// </summary>
    public static class SystemCalendarExtensions
    {

        /// <summary>
        /// Get offset year
        /// </summary>
        /// <param name="year">The gregorian year</param>
        /// <param name="month">The gregorian month</param>
        /// <param name="offsetMonths">The offset months</param>
        /// <param name="calendar">The culture</param>
        /// <param name="dateTimeKind">The date time kind</param>
        public static DateTime GetOffsetDate(this System.Globalization.Calendar calendar, int year, int month,
            int offsetMonths = 0, DateTimeKind dateTimeKind = DateTimeKind.Utc)
        {
            if (year < DateTime.MinValue.Year || year > DateTime.MaxValue.Year)
            {
                throw new ArgumentOutOfRangeException(nameof(year));
            }
            if (month < 1 || month > 12)
            {
                throw new ArgumentOutOfRangeException(nameof(month));
            }

            // base date
            if (offsetMonths == 0)
            {
                return new DateTime(year, month, 1, 0, 0, 0, 0, calendar, dateTimeKind);
            }

            // month
            var targetMonth = month + offsetMonths;

            // multiple day offset
            var diffYears = targetMonth / Date.MonthsInYear;
            year += diffYears;
            targetMonth -= diffYears * Date.MonthsInYear;

            // remaining months within a year
            var diffMonths = (decimal)targetMonth % Date.MonthsInYear;
            if (diffMonths <= 0)
            {
                // subtract months
                year--;
                month = Date.MonthsInYear - decimal.ToInt32(Math.Abs(diffMonths));
            }
            else
            {
                // add months
                month = decimal.ToInt32(diffMonths);
            }

            return new DateTime(year, month, 1, 0, 0, 0, 0, calendar, dateTimeKind);
        }
    }
}
