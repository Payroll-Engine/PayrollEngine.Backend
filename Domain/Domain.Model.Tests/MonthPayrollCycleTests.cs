
namespace PayrollEngine.Domain.Model.Tests
{
    public class MonthPayrollCycleTests
    {
        /*
        [Fact]
        public void SimpleMonthPayrollCycleTest()
        {
            PayrollCalendar calendar = new PayrollCalendar(new CalendarConfiguration { FirstMonthOfYear = Month.January });
            var evaluationPeriod = new YearPayrollCycle(calendar, 2019, 11).Period;
            Assert.Equal(evaluationPeriod.Start, new DateTime(2019, 1, 1));
            Assert.Equal(evaluationPeriod.End, evaluationPeriod.Start.AddYears(1).AddDays(-1).LastMomentOfDay());
        }

        [Fact]
        public void FiscalYearMonthPayrollCycleTest()
        {
            PayrollCalendar calendar = new PayrollCalendar(CultureInfo.CurrentCulture, new CalendarConfiguration { FirstMonthOfYear = Month.June });

            var evaluationPeriod = new YearPayrollCycle(calendar, 2019, 2).Period;
            Assert.Equal(evaluationPeriod.Start, new DateTime(2018, 6, 1));
            Assert.Equal(evaluationPeriod.End, evaluationPeriod.Start.AddYears(1).AddDays(-1).LastMomentOfDay());

            evaluationPeriod = new YearPayrollCycle(calendar, 2019, 11).Period;
            Assert.Equal(evaluationPeriod.Start, new DateTime(2019, 6, 1));
            Assert.Equal(evaluationPeriod.End, evaluationPeriod.Start.AddYears(1).AddDays(-1).LastMomentOfDay());
        }

        [Fact]
        public void CustomMonthPayrollCycleTest()
        {
            PayrollCalendar calendar = new PayrollCalendar(CultureInfo.CurrentCulture, new CalendarConfiguration { FirstMonthOfYear = Month.December });

            var evaluationPeriod = new YearPayrollCycle(calendar, 2019, 11).Period;
            Assert.Equal(evaluationPeriod.Start, new DateTime(2018, 12, 1));
            Assert.Equal(evaluationPeriod.End, evaluationPeriod.Start.AddYears(1).AddDays(-1).LastMomentOfDay());

            evaluationPeriod = new YearPayrollCycle(calendar, 2019, 12).Period;
            Assert.Equal(evaluationPeriod.Start, new DateTime(2019, 12, 1));
            Assert.Equal(evaluationPeriod.End, evaluationPeriod.Start.AddYears(1).AddDays(-1).LastMomentOfDay());

            evaluationPeriod = new YearPayrollCycle(calendar, 2020, 1).Period;
            Assert.Equal(evaluationPeriod.Start, new DateTime(2019, 12, 1));
            Assert.Equal(evaluationPeriod.End, evaluationPeriod.Start.AddYears(1).AddDays(-1).LastMomentOfDay());
        }
        */
    }
}
