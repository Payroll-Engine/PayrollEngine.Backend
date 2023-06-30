using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class NationalCaseValueService : CaseValueService<INationalCaseValueRepository>, INationalCaseValueService
{
    public NationalCaseValueService(INationalCaseValueRepository nationalCaseValueRepository) :
        base(nationalCaseValueRepository)
    {
    }
}