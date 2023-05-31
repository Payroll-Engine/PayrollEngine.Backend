﻿using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the report templates
/// </summary>
public abstract class ReportTemplateController : RepositoryChildObjectController<IReportService, IReportTemplateService,
    IReportRepository, IReportTemplateRepository,
    DomainObject.Report, DomainObject.ReportTemplate, ApiObject.ReportTemplate>
{
    protected ReportTemplateController(IReportService reportService, IReportTemplateService reportTemplateService,
        IControllerRuntime runtime) :
        base(reportService, reportTemplateService, runtime, new ReportTemplateMap())
    {
    }
}