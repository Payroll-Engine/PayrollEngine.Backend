using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public abstract class CaseDocumentService<TRepo> :
    ChildApplicationService<TRepo, CaseDocument>, ICaseDocumentService<TRepo, CaseDocument>
    where TRepo : class, ICaseDocumentRepository
{
    protected CaseDocumentService(TRepo caseDocumentRepository) :
        base(caseDocumentRepository)
    {
    }
}