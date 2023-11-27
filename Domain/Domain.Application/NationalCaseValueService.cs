using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class NationalCaseValueService(INationalCaseValueRepository nationalCaseValueRepository) :
    CaseValueService<INationalCaseValueRepository>(nationalCaseValueRepository), INationalCaseValueService;