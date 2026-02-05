using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

internal sealed class PayrollRepositoryReportCommand : PayrollRepositoryCommandBase
{
    internal PayrollRepositoryReportCommand(IDbContext dbContext) :
        base(dbContext)
    {
    }

    /// <summary>
    /// Get derived payroll reports
    /// </summary>
    /// <param name="reportRepository">The report repository</param>
    /// <param name="query">The query</param>
    /// <param name="reportNames">The report names</param>
    /// <param name="overrideType">The override type</param>
    /// <param name="clusterSet">The cluster set</param>
    /// <param name="userType">The user type</param>
    /// <returns>The derived reports, ordered by derivation level</returns>
    internal async Task<IEnumerable<ReportSet>> GetDerivedReportsAsync(
        IReportSetRepository reportRepository,
        PayrollQuery query, IEnumerable<string> reportNames = null,
        OverrideType? overrideType = null, UserType? userType = null,
        ClusterSet clusterSet = null)
    {
        // argument check
        if (reportRepository == null)
        {
            throw new ArgumentNullException(nameof(reportRepository));
        }
        if (query == null)
        {
            throw new ArgumentNullException(nameof(query));
        }

        if (query.TenantId <= 0)
        {
            throw new ArgumentException(nameof(query.TenantId));
        }
        if (query.PayrollId <= 0)
        {
            throw new ArgumentException(nameof(query.PayrollId));
        }
        var names = reportNames?.Distinct().ToList();
        if (names != null)
        {
            foreach (var name in names)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException(nameof(reportNames));
                }
            }
        }

        // query setup
        query.RegulationDate ??= Date.Now;
        query.EvaluationDate ??= Date.Now;

        // parameters
        var parameters = new DbParameterCollection();
        parameters.Add(DbSchema.ParameterGetDerivedReports.TenantId, query.TenantId, DbType.Int32);
        parameters.Add(DbSchema.ParameterGetDerivedReports.PayrollId, query.PayrollId, DbType.Int32);
        parameters.Add(DbSchema.ParameterGetDerivedReports.RegulationDate, query.RegulationDate, DbType.DateTime2);
        parameters.Add(DbSchema.ParameterGetDerivedReports.CreatedBefore, query.EvaluationDate, DbType.DateTime2);
        if (userType.HasValue)
        {
            parameters.Add(DbSchema.ParameterGetDerivedReports.UserType, userType.Value);
        }
        if (clusterSet != null)
        {
            if (clusterSet.IncludeClusters != null && clusterSet.IncludeClusters.Any())
            {
                parameters.Add(DbSchema.ParameterGetDerivedReports.IncludeClusters,
                    System.Text.Json.JsonSerializer.Serialize(clusterSet.IncludeClusters));
            }
            if (clusterSet.ExcludeClusters != null && clusterSet.ExcludeClusters.Any())
            {
                parameters.Add(DbSchema.ParameterGetDerivedReports.ExcludeClusters,
                    System.Text.Json.JsonSerializer.Serialize(clusterSet.ExcludeClusters));
            }
        }
        if (names != null && names.Any())
        {
            parameters.Add(DbSchema.ParameterGetDerivedReports.ReportNames,
                System.Text.Json.JsonSerializer.Serialize(names));
        }

        // retrieve all derived reports (stored procedure)
        var reports = (await DbContext.QueryAsync<DerivedReport>(DbSchema.Procedures.GetDerivedReports,
            parameters, commandType: CommandType.StoredProcedure)).ToList();

        BuildDerivedReports(reports, overrideType);

        // build report sets
        var reportSets = new List<ReportSet>();
        foreach (var report in reports)
        {
            var reportSet = await reportRepository.GetAsync(DbContext, report.RegulationId, report.Id);
            reportSets.Add(reportSet);
        }

        return reportSets;
    }

    private static void BuildDerivedReports(List<DerivedReport> reports, OverrideType? overrideType = null)
    {
        if (reports == null)
        {
            throw new ArgumentNullException(nameof(reports));
        }
        if (!reports.Any())
        {
            return;
        }

        // resulting reports
        var reportsByKey = reports.GroupBy(x => x.Name).ToList();

        // override filter
        if (overrideType.HasValue)
        {
            ApplyOverrideFilter(reportsByKey, reports, overrideType.Value);
            // update reports
            reportsByKey = reports.GroupBy(x => x.Name).ToList();
        }

        // collect derived values
        foreach (var reportKey in reportsByKey)
        {
            // order by derived reports
            var derivedReports = reportKey.OrderByDescending(x => x.Level).ThenByDescending(x => x.Priority).ToList();
            // derived reports
            while (derivedReports.Count > 1)
            {
                // collect derived values
                var derivedReport = derivedReports.First();
                // non-derived fields: name and all non-nullable and expressions
                derivedReport.NameLocalizations = CollectDerivedValue(derivedReports, x => x.NameLocalizations);
                derivedReport.Description = CollectDerivedValue(derivedReports, x => x.Description);
                derivedReport.DescriptionLocalizations = CollectDerivedValue(derivedReports, x => x.DescriptionLocalizations);
                derivedReport.Category = CollectDerivedValue(derivedReports, x => x.Category);
                derivedReport.Queries = CollectDerivedDictionary(derivedReports, x => x.Queries);
                derivedReport.Relations = CollectDerivedList(derivedReports, x => x.Relations);
                derivedReport.Attributes = CollectDerivedAttributes(derivedReports);
                derivedReport.Clusters = CollectDerivedList(derivedReports, x => x.Clusters);
                // remove the current level for the next iteration
                derivedReports.Remove(derivedReport);
            }
        }
    }
}