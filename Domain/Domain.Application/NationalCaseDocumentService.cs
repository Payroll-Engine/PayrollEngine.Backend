using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class NationalCaseDocumentService : CaseDocumentService<INationalCaseDocumentRepository>, INationalCaseDocumentService
{
    public NationalCaseDocumentService(INationalCaseDocumentRepository nationalCaseDocumentRepository) :
        base(nationalCaseDocumentRepository)
    {
    }
}