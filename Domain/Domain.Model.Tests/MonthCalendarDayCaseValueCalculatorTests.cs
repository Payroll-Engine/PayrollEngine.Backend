

namespace PayrollEngine.Domain.Model.Tests
{
    public class MonthCalendarDayCaseValueCalculatorTests
    {
        /*
        private PayrollCalendar Calendar => new PayrollCalendar(CultureInfo.CurrentCulture, new CalendarConfiguration());

        [Fact]
        public void SinglePeriodValueTest()
        {
            var evaluationPeriod = new MonthPayrollPeriod(Calendar, 2019, 12).Period;
            var calculation = new CasePeriodValueCalculation
            {
                EvaluationDate = Date.Now,
                EvaluationPeriod = evaluationPeriod,
                CaseValuePeriod = evaluationPeriod,
                CaseValue = 2000
            };

            var calculator = new MonthCalendarDayPayrollCalculator(Calendar);
            var result = calculator.CalculateCasePeriodValue(calculation);
            Assert.NotNull(result);
            Assert.Equal(2000, result);
        }

        [Fact]
        public void DoublePeriodValueTest()
        {
            var evaluationPeriod = new MonthPayrollPeriod(Calendar, 2019, 11).Period;
            var calculation = new CasePeriodValueCalculation
            {
                EvaluationDate = Date.Now,
                EvaluationPeriod = evaluationPeriod,
                CaseValuePeriod = evaluationPeriod,
                CaseValue = 2000
            };

            var calculator = new MonthCalendarDayPayrollCalculator(Calendar);
            var startDate = evaluationPeriod.Start;

            // period 1: first half
            calculation.CaseValuePeriod = new DatePeriod(startDate, startDate.AddDays(15));
            var result1 = calculator.CalculateCasePeriodValue(calculation);
            Assert.NotNull(result1);
            Assert.Equal((decimal)2000 / 2, result1);

            // period 2: second half
            calculation.CaseValuePeriod = new DatePeriod(startDate.AddDays(15), startDate.AddDays(30));
            var result2 = calculator.CalculateCasePeriodValue(calculation);
            Assert.NotNull(result2);
            Assert.True(((decimal)2000 / 2).AlmostEquals(result2.Value, 2));
        }

        [Fact]
        public void DoublePeriodWithDifferentValuesTest()
        {
            var evaluationPeriod = new MonthPayrollPeriod(Calendar, 2019, 11).Period;
            var calculation = new CasePeriodValueCalculation
            {
                EvaluationDate = Date.Now,
                EvaluationPeriod = evaluationPeriod,
                CaseValuePeriod = evaluationPeriod,
                CaseValue = 2000
            };

            var calculator = new MonthCalendarDayPayrollCalculator(Calendar);
            var startDate = evaluationPeriod.Start;

            // period 1: first 10 days
            calculation.CaseValuePeriod = new DatePeriod(evaluationPeriod.Start, Date.LastMomentOfDay(startDate.AddDays(9)));
            var result1 = calculator.CalculateCasePeriodValue(calculation);
            Assert.NotNull(result1);
            Assert.True(((decimal)2000 / 30 * 10).AlmostEquals(result1.Value, 2));

            // period 2: last 10 days
            calculation.CaseValue = 2400;
            calculation.CaseValuePeriod =
                new DatePeriod(evaluationPeriod.Start.AddDays(20), Date.LastMomentOfDay(evaluationPeriod.End));
            var result2 = calculator.CalculateCasePeriodValue(calculation);
            Assert.NotNull(result2);
            Assert.True(((decimal)2400 / 30 * 10).AlmostEquals(result2.Value, 2));
        }

        [Fact]
        public void SingleFractionPeriodValueTest()
        {
            var evaluationPeriod = new MonthPayrollPeriod(Calendar, 2019, 12).Period;
            var calculation = new CasePeriodValueCalculation
            {
                EvaluationDate = Date.Now,
                EvaluationPeriod = evaluationPeriod,
                CaseValuePeriod = new DatePeriod(new DateTime(2019, 12, 3), new DateTime(2019, 12, 3).AddDays(10)),
                CaseValue = 2000
            };

            var calculator = new MonthCalendarDayPayrollCalculator(Calendar);
            var result = calculator.CalculateCasePeriodValue(calculation);
            Assert.NotNull(result);
            Assert.True(((decimal)2000 / 31 * 10).AlmostEquals(result.Value, 2));
        }

        */
    }
}
