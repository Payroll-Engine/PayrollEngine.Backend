using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Extension methods for <see cref="IPayrollPeriod"/>
/// </summary>
public static class PayrollPeriodExtensions
{
    /// <param name="payrollPeriod">The payrun period</param>
    extension(IPayrollPeriod payrollPeriod)
    {
        /// <summary>Get date period</summary>
        /// <returns>Date period</returns>
        public DatePeriod GetDatePeriod() =>
            new(payrollPeriod.Start, payrollPeriod.End);

        /// <summary>
        /// Get offset period
        /// </summary>
        /// <param name="offset">The period offset count</param>
        /// <returns>Offset period</returns>
        public DatePeriod GetOffsetDatePeriod(int offset)=>
            payrollPeriod.GetPayrollPeriod(payrollPeriod.Start, offset).GetDatePeriod();

        /// <summary>
        /// Get all periods between this period and the period containing the target moment
        /// The starting payrun period is not included
        /// The payrun period containing the target moment is included
        /// </summary>
        /// <param name="targetMoment">Target moment</param>
        /// <param name="maxCount">Maximum result count</param>
        /// <returns>Payrun periods between this and the target moment,
        /// sorted from the oldest to the newest one</returns>
        public IList<IPayrollPeriod> GetContinuePeriods(DateTime targetMoment, int maxCount)
        {
            if (maxCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxCount));
            }

            var datePeriod = payrollPeriod.GetDatePeriod();
            var periods = new List<IPayrollPeriod>();
            if (datePeriod.IsWithin(targetMoment))
            {
                return periods;
            }

            // return the period to continue
            var previous = targetMoment < payrollPeriod.Start;
            IPayrollPeriod ContinuePeriod(IPayrollPeriod source) =>
                previous ? source.GetPayrollPeriod(source.Start, -1) : source.GetPayrollPeriod(source.Start, 1);

            var period = payrollPeriod;
            while (true)
            {
                period = ContinuePeriod(period);
                periods.Add(period);

                // target period
                if (datePeriod.IsWithin(targetMoment))
                {
                    break;
                }

                // limits
                if (periods.Count >= maxCount)
                {
                    throw new PayrollException($"To many periods ({maxCount}) for " +
                                               $"the payrun period {datePeriod} with target date {targetMoment}.");
                }
            }

            return periods;
        }
    }
}