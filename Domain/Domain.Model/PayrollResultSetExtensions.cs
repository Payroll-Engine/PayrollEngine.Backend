using System;
using System.Collections.Generic;
using System.Linq;
using PayrollEngine.Data;

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
        /// Converts the payroll result set to a <see cref="DataSet"/> with tables
        /// for wage type results, collector results, payrun results, and their custom results.
        /// </summary>
        /// <returns>A <see cref="DataSet"/> containing the payroll calculation results.</returns>
        public DataSet ToDataSet()
        {
            var dataSet = new DataSet("PayrunPreview")
            {
                Tables =
                [
                    (payrollResultSet.WageTypeResults ?? []).ToPayrollDataTable("WageTypeResult"),
                    (payrollResultSet.WageTypeResults ?? [])
                        .Where(w => w.CustomResults != null)
                        .SelectMany(w => w.CustomResults)
                        .ToPayrollDataTable("WageTypeCustomResult"),

                    (payrollResultSet.CollectorResults ?? []).ToPayrollDataTable("CollectorResult"),
                    (payrollResultSet.CollectorResults ?? [])
                        .Where(c => c.CustomResults != null)
                        .SelectMany(c => c.CustomResults)
                        .ToPayrollDataTable("CollectorCustomResult"),

                    (payrollResultSet.PayrunResults ?? []).ToPayrollDataTable("PayrunResult")
                ],
                Relations =
                [
                    new DataRelation
                    {
                        Name = "WageTypeResult_CustomResult",
                        ParentTable = "WageTypeResult",
                        ParentColumn = "WageTypeNumber",
                        ChildTable = "WageTypeCustomResult",
                        ChildColumn = "WageTypeNumber"
                    },
                    new DataRelation
                    {
                        Name = "CollectorResult_CustomResult",
                        ParentTable = "CollectorResult",
                        ParentColumn = "CollectorName",
                        ChildTable = "CollectorCustomResult",
                        ChildColumn = "CollectorName"
                    }
                ]
            };

            return dataSet;
        }

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
        /// Set denormalized context columns on all results.
        /// These columns are redundant copies from PayrollResult/PayrunJob for direct index seeks.
        /// Phase 1: TenantId, EmployeeId, DivisionId
        /// Phase 2: PayrunJobId, Forecast, ParentJobId
        /// </summary>
        /// <param name="tenantId">The tenant id</param>
        /// <param name="employeeId">The employee id</param>
        /// <param name="divisionId">The division id</param>
        /// <param name="payrunJobId">The payrun job id</param>
        /// <param name="forecast">The forecast name</param>
        /// <param name="parentJobId">The parent payrun job id</param>
        public void SetDenormalizedContext(int tenantId, int employeeId, int divisionId,
            int payrunJobId, string forecast, int? parentJobId)
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
                    collectorResult.TenantId = tenantId;
                    collectorResult.EmployeeId = employeeId;
                    collectorResult.DivisionId = divisionId;
                    collectorResult.PayrunJobId = payrunJobId;
                    collectorResult.Forecast = forecast;
                    collectorResult.ParentJobId = parentJobId;

                    // collector custom results
                    if (collectorResult is { } resultSet)
                    {
                        if (resultSet.CustomResults != null)
                        {
                            foreach (var customResult in resultSet.CustomResults)
                            {
                                customResult.TenantId = tenantId;
                                customResult.EmployeeId = employeeId;
                                customResult.DivisionId = divisionId;
                                customResult.PayrunJobId = payrunJobId;
                                customResult.Forecast = forecast;
                                customResult.ParentJobId = parentJobId;
                            }
                        }
                    }
                }
            }

            // wage type results
            if (payrollResultSet.WageTypeResults != null)
            {
                foreach (var wageTypeResult in payrollResultSet.WageTypeResults)
                {
                    wageTypeResult.TenantId = tenantId;
                    wageTypeResult.EmployeeId = employeeId;
                    wageTypeResult.DivisionId = divisionId;
                    wageTypeResult.PayrunJobId = payrunJobId;
                    wageTypeResult.Forecast = forecast;
                    wageTypeResult.ParentJobId = parentJobId;

                    // wage type custom results
                    if (wageTypeResult.CustomResults != null)
                    {
                        foreach (var customResult in wageTypeResult.CustomResults)
                        {
                            customResult.TenantId = tenantId;
                            customResult.EmployeeId = employeeId;
                            customResult.DivisionId = divisionId;
                            customResult.PayrunJobId = payrunJobId;
                            customResult.Forecast = forecast;
                            customResult.ParentJobId = parentJobId;
                        }
                    }
                }
            }

            // payrun results
            if (payrollResultSet.PayrunResults != null)
            {
                foreach (var payrunResult in payrollResultSet.PayrunResults)
                {
                    payrunResult.TenantId = tenantId;
                    payrunResult.EmployeeId = employeeId;
                    payrunResult.DivisionId = divisionId;
                    payrunResult.PayrunJobId = payrunJobId;
                    payrunResult.Forecast = forecast;
                    payrunResult.ParentJobId = parentJobId;
                }
            }
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