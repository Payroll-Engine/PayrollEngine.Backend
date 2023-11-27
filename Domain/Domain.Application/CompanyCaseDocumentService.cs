using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class CompanyCaseDocumentService(ICompanyCaseDocumentRepository companyCaseDocumentRepository)
    : CaseDocumentService<ICompanyCaseDocumentRepository>(companyCaseDocumentRepository), ICompanyCaseDocumentService;