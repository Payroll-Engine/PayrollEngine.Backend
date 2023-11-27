using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class CompanyCaseValueService(ICompanyCaseValueRepository companyCaseValueRepository) :
    CaseValueService<ICompanyCaseValueRepository>(companyCaseValueRepository), ICompanyCaseValueService;