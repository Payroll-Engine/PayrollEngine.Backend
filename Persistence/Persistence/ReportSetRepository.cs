﻿using System;
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

    public ReportSetRepository(ReportSetRepositorySettings settings) :
        base(settings.ScriptController, settings.ScriptRepository, settings.AuditRepository)
    {
        Settings = settings;
        ReportParameterRepository = settings.ReportParameterRepository ??
                                    throw new ArgumentNullException(nameof(settings.ReportParameterRepository));
        ReportTemplateRepository = settings.ReportTemplateRepository ??
                                   throw new ArgumentNullException(nameof(settings.ReportTemplateRepository));
    }

    protected override async Task OnRetrieved(IDbContext context, int regulationId, ReportSet reportSet)
    {
        reportSet.RegulationId = regulationId;

        // report parameters
        reportSet.Parameters = (await ReportParameterRepository.QueryAsync(context, reportSet.Id)).ToList();

        // report templates, exclude potential large content from the report set
        reportSet.Templates = (await ReportTemplateRepository.QueryAsync(context, reportSet.Id,
            new ReportTemplateQuery { ExcludeContent = true })).ToList();

        await base.OnRetrieved(context, regulationId, reportSet);
    }

    protected override async Task OnCreatedAsync(IDbContext context, int regulationId, ReportSet reportSet)
    {
        reportSet.RegulationId = regulationId;

        // report parameters
        if (reportSet.Parameters != null && reportSet.Parameters.Any())
        {
            if (BulkInsert)
            {
                await ReportParameterRepository.CreateBulkAsync(context, reportSet.Id, reportSet.Parameters);
            }
            else
            {
                await ReportParameterRepository.CreateAsync(context, reportSet.Id, reportSet.Parameters);
            }
        }

        // report templates
        if (reportSet.Templates != null && reportSet.Templates.Any())
        {
            if (BulkInsert)
            {
                await ReportTemplateRepository.CreateBulkAsync(context, reportSet.Id, reportSet.Templates);
            }
            else
            {
                await ReportTemplateRepository.CreateAsync(context, reportSet.Id, reportSet.Templates);
            }
        }

        await base.OnCreatedAsync(context, regulationId, reportSet);
    }

    protected override Task OnUpdatedAsync(IDbContext context, int regulationId, ReportSet report)
    {
        throw new NotSupportedException("Update of report set not supported, please use the report parameter container");
    }

    protected override async Task<bool> OnDeletingAsync(IDbContext context, int regulationId, int resultId)
    {
        // report templates
        var templates = (await ReportTemplateRepository.QueryAsync(context, resultId)).ToList();
        foreach (var template in templates)
        {
            await ReportTemplateRepository.DeleteAsync(context, resultId, template.Id);
        }

        // report parameters
        var parameters = (await ReportParameterRepository.QueryAsync(context, resultId)).ToList();
        foreach (var parameter in parameters)
        {
            await ReportParameterRepository.DeleteAsync(context, resultId, parameter.Id);
        }

        return await base.OnDeletingAsync(context, regulationId, resultId);
    }
}