using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class ReportTemplateService(IReportTemplateRepository repository) :
    ChildApplicationService<IReportTemplateRepository, ReportTemplate>(repository), IReportTemplateService;