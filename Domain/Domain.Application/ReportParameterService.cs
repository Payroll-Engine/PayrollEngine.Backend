using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class ReportParameterService(IReportParameterRepository repository) :
    ChildApplicationService<IReportParameterRepository, ReportParameter>(repository), IReportParameterService;