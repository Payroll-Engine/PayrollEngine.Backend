using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Task = System.Threading.Tasks.Task;
using PayrollEngine.Domain.Model;
using PayrollEngine.Serialization;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class ReportTemplateRepository(IRegulationRepository regulationRepository,
    IReportRepository reportRepository, IReportTemplateAuditRepository auditRepository, bool auditDisabled) :
    TrackChildDomainRepository<ReportTemplate, ReportTemplateAudit>(regulationRepository,
        DbSchema.Tables.ReportTemplate, DbSchema.ReportTemplateColumn.ReportId,
        auditRepository, auditDisabled), IReportTemplateRepository
{
    private IReportRepository ReportRepository { get; } = reportRepository ?? throw new ArgumentNullException(nameof(reportRepository));

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
        parameters.Add(nameof(template.OverrideType), template.OverrideType, DbType.Int32);
        parameters.Add(nameof(template.Attributes), JsonSerializer.SerializeNamedDictionary(template.Attributes));
        base.GetObjectData(template, parameters);
    }

    /// <inheritdoc />
    public override async Task<IEnumerable<ReportTemplate>> QueryAsync(IDbContext context, int reportId, Query query = null)
    {
        // report template query
        if (query is ReportTemplateQuery reportTemplateQuery && !string.IsNullOrWhiteSpace(reportTemplateQuery.Culture))
        {
            // db query
            var dbQuery = GetTemplateQuery(context, reportId, reportTemplateQuery);

            // T-SQL SELECT execution
            var reportTemplates = (await QueryAsync<ReportTemplate>(context, dbQuery)).ToList();

            // notification
            await OnRetrieved(context, reportId, reportTemplates);

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

        return await base.QueryAsync(context, reportId, query);
    }

    /// <inheritdoc />
    public override async Task<long> QueryCountAsync(IDbContext context, int reportId, Query query = null)
    {
        // report template query
        if (query is ReportTemplateQuery reportTemplateQuery && !string.IsNullOrWhiteSpace(reportTemplateQuery.Culture))
        {
            // db query
            var dbQuery = GetTemplateQuery(context, reportId, reportTemplateQuery);
            return await QuerySingleAsync<long>(context, dbQuery);
        }
        return await base.QueryCountAsync(context, reportId, query);
    }

    public override async Task<ReportTemplate> CreateAsync(IDbContext context, int reportId, ReportTemplate template)
    {
        await EnsureNamespaceAsync(context, reportId, template);
        return await base.CreateAsync(context, reportId, template);
    }

    public override async Task<ReportTemplate> UpdateAsync(IDbContext context, int reportId, ReportTemplate template)
    {
        await EnsureNamespaceAsync(context, reportId, template);
        return await base.UpdateAsync(context, reportId, template);
    }

    private async Task EnsureNamespaceAsync(IDbContext context, int reportId, ReportTemplate template)
    {
        var regulationId = await ReportRepository.GetParentIdAsync(context, reportId);
        if (!regulationId.HasValue)
        {
            return;
        }
        await ApplyNamespaceAsync(context, regulationId.Value, template);
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