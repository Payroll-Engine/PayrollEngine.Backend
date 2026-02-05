using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence;

internal sealed class PayrollRepositoryReportTemplatesCommand : PayrollRepositoryCommandBase
{
    internal PayrollRepositoryReportTemplatesCommand(IDbContext dbContext) :
        base(dbContext)
    {
    }

    internal async Task<IEnumerable<DerivedReportTemplate>> GetDerivedReportTemplatesAsync(PayrollQuery query,
        IEnumerable<string> reportNames = null, string culture = null, OverrideType? overrideType = null)
    {
        // query check
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
        parameters.Add(DbSchema.ParameterGetDerivedReportTemplates.TenantId, query.TenantId, DbType.Int32);
        parameters.Add(DbSchema.ParameterGetDerivedReportTemplates.PayrollId, query.PayrollId, DbType.Int32);
        parameters.Add(DbSchema.ParameterGetDerivedReportTemplates.RegulationDate, query.RegulationDate, DbType.DateTime2);
        parameters.Add(DbSchema.ParameterGetDerivedReportTemplates.CreatedBefore, query.EvaluationDate, DbType.DateTime2);
        if (names != null && names.Any())
        {
            parameters.Add(DbSchema.ParameterGetDerivedReportTemplates.ReportNames,
                System.Text.Json.JsonSerializer.Serialize(names));
        }
        // culture
        if (!String.IsNullOrWhiteSpace(culture))
        {
            parameters.Add(DbSchema.ParameterGetDerivedReportTemplates.Culture, culture);
        }

        // retrieve all derived report templates (stored procedure)
        var reportTemplates = (await DbContext.QueryAsync<DerivedReportTemplate>(DbSchema.Procedures.GetDerivedReportTemplates,
            parameters, commandType: CommandType.StoredProcedure)).ToList();

        BuildDerivedReportTemplates(reportTemplates, overrideType);
        return reportTemplates;
    }

    private static void BuildDerivedReportTemplates(List<DerivedReportTemplate> reportTemplates, OverrideType? overrideType = null)
    {
        if (reportTemplates == null)
        {
            throw new ArgumentNullException(nameof(reportTemplates));
        }
        if (!reportTemplates.Any())
        {
            return;
        }

        // resulting reports
        var reportTemplatesByKey = reportTemplates.GroupBy(x => x.Name).ToList();

        // override filter
        if (overrideType.HasValue)
        {
            ApplyOverrideFilter(reportTemplatesByKey, reportTemplates, overrideType.Value);
            // update reports
            reportTemplatesByKey = reportTemplates.GroupBy(x => x.Name).ToList();
        }

        // collect derived values
        foreach (var reportKey in reportTemplatesByKey)
        {
            // order by derived reports
            var derivedTemplates = reportKey.OrderByDescending(x => x.Level).ThenByDescending(x => x.Priority).ToList();
            // derived reports
            while (derivedTemplates.Count > 1)
            {
                // collect derived values
                var derivedTemplate = derivedTemplates.First();
                // language override
                // non-derived fields: name and culture
                derivedTemplate.Content = CollectDerivedValue(derivedTemplates, x => x.Content);
                derivedTemplate.ContentType = CollectDerivedValue(derivedTemplates, x => x.ContentType);
                derivedTemplate.Schema = CollectDerivedValue(derivedTemplates, x => x.Schema);
                derivedTemplate.Resource = CollectDerivedValue(derivedTemplates, x => x.Resource);
                derivedTemplate.Attributes = CollectDerivedAttributes(derivedTemplates);
                // remove the current level for the next iteration
                derivedTemplates.Remove(derivedTemplate);
            }
        }
    }
}