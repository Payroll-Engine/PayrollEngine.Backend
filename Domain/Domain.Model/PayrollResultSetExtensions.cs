using System;
using System.Collections.Generic;
using System.Linq;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Payroll result extensions
/// </summary>
public static class PayrollResultSetExtensions
{
    /// <param name="payrollResultSet">The payroll result set</param>
    extension(PayrollResultSet payrollResultSet)
    {
        /// <summary>
        /// Test for empty result set
        /// </summary>
        public bool IsEmpty()
        {
            // payrun results
            if (payrollResultSet.PayrunResults.Any(result => !string.IsNullOrWhiteSpace(result.Value)))
            {
                return false;
            }

            // collector results
            if (payrollResultSet.CollectorResults.Any(result => result.Value != 0))
            {
                return false;
            }

            // wage type results
            return payrollResultSet.WageTypeResults.All(result => result.Value == 0);
        }

        /// <summary>
        /// Set creation date for all results
        /// </summary>
        /// <param name="moment">The moment to set</param>
        public void SetResultDate(DateTime moment)
        {
            if (payrollResultSet == null)
            {
                throw new ArgumentNullException(nameof(payrollResultSet));
            }

            // collector results
            if (payrollResultSet.CollectorResults != null)
            {
                foreach (var collectorResult in payrollResultSet.CollectorResults)
                {
                    collectorResult.SetCreatedDate(moment);
                }
            }

            // wage type results
            if (payrollResultSet.WageTypeResults != null)
            {
                foreach (var wageTypeResult in payrollResultSet.WageTypeResults)
                {
                    wageTypeResult.SetCreatedDate(moment);

                    // wage type custom results
                    if (wageTypeResult.CustomResults != null)
                    {
                        foreach (var customResult in wageTypeResult.CustomResults)
                        {
                            customResult.SetCreatedDate(moment);
                        }
                    }
                }
            }

            // payrun results
            if (payrollResultSet.PayrunResults != null)
            {
                foreach (var payrunResult in payrollResultSet.PayrunResults)
                {
                    payrunResult.SetCreatedDate(moment);
                }
            }
        }

        /// <summary>
        /// Add tags to all results
        /// </summary>
        /// <param name="tags">The tags to add</param>
        public void AddTags(List<string> tags)
        {
            if (payrollResultSet == null)
            {
                throw new ArgumentNullException(nameof(payrollResultSet));
            }
            if (tags == null)
            {
                throw new ArgumentNullException(nameof(tags));
            }
            if (!tags.Any())
            {
                throw new ArgumentException(nameof(tags));
            }

            // collector results
            if (payrollResultSet.CollectorResults != null)
            {
                AddCollectorResultTags(payrollResultSet, tags);
            }

            // wage type results
            if (payrollResultSet.WageTypeResults != null)
            {
                AddWageTypeResultTags(payrollResultSet, tags);
            }

            // payrun results
            if (payrollResultSet.PayrunResults != null)
            {
                AddPayrunResultTags(payrollResultSet, tags);
            }
        }
    }

    private static void AddCollectorResultTags(PayrollResultSet payrollResultSet, List<string> tags)
    {
        foreach (var collectorResult in payrollResultSet.CollectorResults)
        {
            collectorResult.Tags ??= [];
            collectorResult.Tags.AddNew(tags);
        }
    }

    private static void AddWageTypeResultTags(PayrollResultSet payrollResultSet, List<string> tags)
    {
        foreach (var wageTypeResult in payrollResultSet.WageTypeResults)
        {
            wageTypeResult.Tags ??= [];
            wageTypeResult.Tags.AddNew(tags);

            // wage type custom results
            if (wageTypeResult.CustomResults != null)
            {
                foreach (var customResult in wageTypeResult.CustomResults)
                {
                    customResult.Tags ??= [];
                    customResult.Tags.AddNew(tags);
                }
            }
        }
    }

    private static void AddPayrunResultTags(PayrollResultSet payrollResultSet, List<string> tags)
    {
        foreach (var payrunResult in payrollResultSet.PayrunResults)
        {
            payrunResult.Tags ??= [];
            payrunResult.Tags.AddNew(tags);
        }
    }
}