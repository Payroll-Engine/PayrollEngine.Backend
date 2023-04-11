using System;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Persistence;

public class ReportSetRepository : ReportRepositoryBase<ReportSet>, IReportSetRepository
{
    public ReportSetRepositorySettings Settings { get; }
    public IReportParameterRepository ReportParameterRepository { get; }
    public IReportTemplateRepository ReportTemplateRepository { get; }
    public bool BulkInsert => Settings.BulkInsert;

    public ReportSetRepository(ReportSetRepositorySettings settings, IDbContext context) :
        base(settings.ScriptController, settings.ScriptRepository, settings.AuditRepository, context)
    {
        Settings = settings;
        ReportParameterRepository = settings.ReportParameterRepository ??
                                    throw new ArgumentNullException(nameof(settings.ReportParameterRepository));
        ReportTemplateRepository = settings.ReportTemplateRepository ??
                                   throw new ArgumentNullException(nameof(settings.ReportTemplateRepository));
    }

    protected override async Task OnRetrieved(int regulationId, ReportSet reportSet)
    {
        reportSet.RegulationId = regulationId;

        // report parameters
        reportSet.Parameters = (await ReportParameterRepository.QueryAsync(reportSet.Id)).ToList();

        // report templates, exclude potential large content from the report set
        reportSet.Templates = (await ReportTemplateRepository.QueryAsync(reportSet.Id,
            new ReportTemplateQuery { ExcludeContent = true })).ToList();

        await base.OnRetrieved(regulationId, reportSet);
    }

    protected override async Task OnCreatedAsync(int regulationId, ReportSet reportSet)
    {
        reportSet.RegulationId = regulationId;

        // report parameters
        if (reportSet.Parameters != null && reportSet.Parameters.Any())
        {
            if (BulkInsert)
            {
                await ReportParameterRepository.CreateBulkAsync(reportSet.Id, reportSet.Parameters);
            }
            else
            {
                await ReportParameterRepository.CreateAsync(reportSet.Id, reportSet.Parameters);
            }
        }

        // report templates
        if (reportSet.Templates != null && reportSet.Templates.Any())
        {
            if (BulkInsert)
            {
                await ReportTemplateRepository.CreateBulkAsync(reportSet.Id, reportSet.Templates);
            }
            else
            {
                await ReportTemplateRepository.CreateAsync(reportSet.Id, reportSet.Templates);
            }
        }

        await base.OnCreatedAsync(regulationId, reportSet);
    }

    protected override Task OnUpdatedAsync(int regulationId, ReportSet report)
    {
        throw new NotSupportedException("Update of report set not supported, please use the report parameter container");
    }

    protected override async Task<bool> OnDeletingAsync(int regulationId, int resultId)
    {
        // report templates
        var templates = (await ReportTemplateRepository.QueryAsync(resultId)).ToList();
        foreach (var template in templates)
        {
            await ReportTemplateRepository.DeleteAsync(resultId, template.Id);
        }

        // report parameters
        var parameters = (await ReportParameterRepository.QueryAsync(resultId)).ToList();
        foreach (var parameter in parameters)
        {
            await ReportParameterRepository.DeleteAsync(resultId, parameter.Id);
        }

        return await base.OnDeletingAsync(regulationId, resultId);
    }
}