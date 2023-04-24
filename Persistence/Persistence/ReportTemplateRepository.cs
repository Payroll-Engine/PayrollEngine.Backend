using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;

namespace PayrollEngine.Persistence;

public class ReportTemplateRepository : TrackChildDomainRepository<ReportTemplate, ReportTemplateAudit>, IReportTemplateRepository
{
    public ReportTemplateRepository(IReportTemplateAuditRepository auditRepository) :
        base(DbSchema.Tables.ReportTemplate, DbSchema.ReportTemplateColumn.ReportId, auditRepository)
    {
    }

    /// <inheritdoc />
    protected override void GetObjectData(ReportTemplate parameter, DbParameterCollection parameters)
    {
        parameters.Add(nameof(parameter.Language), parameter.Language);
        parameters.Add(nameof(parameter.Content), parameter.Content);
        parameters.Add(nameof(parameter.ContentType), parameter.ContentType);
        parameters.Add(nameof(parameter.Schema), parameter.Schema);
        parameters.Add(nameof(parameter.Resource), parameter.Resource);
        parameters.Add(nameof(parameter.Attributes), JsonSerializer.SerializeNamedDictionary(parameter.Attributes));
        base.GetObjectData(parameter, parameters);
    }

    /// <inheritdoc />
    public override async Task<IEnumerable<ReportTemplate>> QueryAsync(IDbContext context, int regulationId, Query query = null)
    {
        // report template query
        if (query is ReportTemplateQuery reportTemplateQuery && reportTemplateQuery.Language.HasValue)
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
        if (query is ReportTemplateQuery reportTemplateQuery && reportTemplateQuery.Language.HasValue)
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