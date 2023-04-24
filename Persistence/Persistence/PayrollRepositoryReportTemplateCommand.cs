using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence;

internal sealed class PayrollRepositoryReportTemplateCommand : PayrollRepositoryCommandBase
{
    internal PayrollRepositoryReportTemplateCommand(IDbContext dbContext) :
        base(dbContext)
    {
    }

    internal async Task<ReportTemplate> GetDerivedReportTemplateAsync(PayrollQuery query,
        Language language, IEnumerable<string> reportNames = null)
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
        parameters.Add(DbSchema.ParameterGetDerivedReportTemplates.TenantId, query.TenantId);
        parameters.Add(DbSchema.ParameterGetDerivedReportTemplates.PayrollId, query.PayrollId);
        parameters.Add(DbSchema.ParameterGetDerivedReportTemplates.RegulationDate, query.RegulationDate);
        parameters.Add(DbSchema.ParameterGetDerivedReportTemplates.CreatedBefore, query.EvaluationDate);
        if (names != null && names.Any())
        {
            parameters.Add(DbSchema.ParameterGetDerivedReportTemplates.ReportNames,
                System.Text.Json.JsonSerializer.Serialize(names));
        }
        parameters.Add(DbSchema.ParameterGetDerivedReportTemplates.Language, (int)language);

        // retrieve all derived report templates (stored procedure)
        var reportTemplates = (await DbContext.QueryAsync<ReportTemplate>(DbSchema.Procedures.GetDerivedReportTemplates,
            parameters, commandType: CommandType.StoredProcedure)).ToList();
        return reportTemplates.FirstOrDefault();
    }
}