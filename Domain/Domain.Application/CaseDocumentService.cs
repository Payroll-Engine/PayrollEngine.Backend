using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public abstract class CaseDocumentService<TRepo>(TRepo caseDocumentRepository) :
    ChildApplicationService<TRepo, CaseDocument>(caseDocumentRepository), ICaseDocumentService<TRepo, CaseDocument>
    where TRepo : class, ICaseDocumentRepository;