using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class CompanyCaseDocumentService : CaseDocumentService<ICompanyCaseDocumentRepository>, ICompanyCaseDocumentService
{
    public CompanyCaseDocumentService(ICompanyCaseDocumentRepository companyCaseDocumentRepository) :
        base(companyCaseDocumentRepository)
    {
    }
}