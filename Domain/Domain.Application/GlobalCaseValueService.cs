using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class GlobalCaseValueService(IGlobalCaseValueRepository globalCaseValueRepository) :
    CaseValueService<IGlobalCaseValueRepository>(globalCaseValueRepository), IGlobalCaseValueService;