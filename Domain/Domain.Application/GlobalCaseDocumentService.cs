using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class GlobalCaseDocumentService : CaseDocumentService<IGlobalCaseDocumentRepository>, IGlobalCaseDocumentService
{
    public GlobalCaseDocumentService(IGlobalCaseDocumentRepository globalCaseDocumentRepository) :
        base(globalCaseDocumentRepository)
    {
    }
}