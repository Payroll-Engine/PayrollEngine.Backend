using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class ReportTemplateService : ChildApplicationService<IReportTemplateRepository, ReportTemplate>, IReportTemplateService
{
    public ReportTemplateService(IReportTemplateRepository repository) :
        base(repository)
    {
    }
}