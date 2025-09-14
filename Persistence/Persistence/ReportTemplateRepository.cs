using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;

namespace PayrollEngine.Persistence;

public class ReportTemplateRepository(IReportTemplateAuditRepository auditRepository, bool auditDisabled) :
    TrackChildDomainRepository<ReportTemplate, ReportTemplateAudit>(DbSchema.Tables.ReportTemplate,
        DbSchema.ReportTemplateColumn.ReportId, auditRepository, auditDisabled), IReportTemplateRepository
{
    /// <inheritdoc />
    protected override void GetObjectCreateData(ReportTemplate template, DbParameterCollection parameters)
    {
        parameters.Add(nameof(template.Name), template.Name);
        base.GetObjectCreateData(template, parameters);
    }

    /// <inheritdoc />
    protected override void GetObjectData(ReportTemplate template, DbParameterCollection parameters)
    {
        parameters.Add(nameof(template.Culture), template.Culture);
        parameters.Add(nameof(template.Content), template.Content);
        parameters.Add(nameof(template.ContentType), template.ContentType);
        parameters.Add(nameof(template.Schema), template.Schema);
        parameters.Add(nameof(template.Resource), template.Resource);
        parameters.Add(nameof(template.OverrideType), template.OverrideType);
        parameters.Add(nameof(template.Attributes), JsonSerializer.SerializeNamedDictionary(template.Attributes));
        base.GetObjectData(template, parameters);
    }

    /// <inheritdoc />
    public override async Task<IEnumerable<ReportTemplate>> QueryAsync(IDbContext context, int regulationId, Query query = null)
    {
        // report template query
        if (query is ReportTemplateQuery reportTemplateQuery && !string.IsNullOrWhiteSpace(reportTemplateQuery.Culture))
        {
            // db query
            var dbQuery = GetTemplateQuery(context, regulationId, reportTemplateQuery);

            // T-SQL SELECT execution
            var reportTemplates = (await QueryAsync<ReportTemplate>(context, dbQuery)).ToList();

            // notification
            await OnRetrieved(context, regulationId, reportTemplates);

            // exclude content
            if (reportTemplateQuery.ExcludeContent)
            {
                // reset report definitions
                foreach (var reportTemplate in reportTemplates)
                {
                    reportTemplate.Content = null;
                }
            }

            return reportTemplates;
        }

        return await base.QueryAsync(context, regulationId, query);
    }

    /// <inheritdoc />
    public override async Task<long> QueryCountAsync(IDbContext context, int regulationId, Query query = null)
    {
        // report template query
        if (query is ReportTemplateQuery reportTemplateQuery && !string.IsNullOrWhiteSpace(reportTemplateQuery.Culture))
        {
            // db query
            var dbQuery = GetTemplateQuery(context, regulationId, reportTemplateQuery);
            return await QuerySingleAsync<long>(context, dbQuery);
        }
        return await base.QueryCountAsync(context, regulationId, query);
    }

    private string GetTemplateQuery(IDbContext context, int regulationId, ReportTemplateQuery query)
    {
        var dbQuery = DbQueryFactory.NewQuery<ReportTemplate>(context, TableName, ParentFieldName, regulationId, query);
        query.ApplyTo(dbQuery);

        // query compilation
        var compileQuery = CompileQuery(dbQuery);
        return compileQuery;
    }
}