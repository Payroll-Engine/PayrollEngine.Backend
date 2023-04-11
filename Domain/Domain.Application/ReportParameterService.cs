using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class ReportParameterService : ChildApplicationService<IReportParameterRepository, ReportParameter>, IReportParameterService
{
    public ReportParameterService(IReportParameterRepository repository) :
        base(repository)
    {
    }
}