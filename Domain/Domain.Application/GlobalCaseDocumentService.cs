using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class GlobalCaseDocumentService(IGlobalCaseDocumentRepository globalCaseDocumentRepository) :
    CaseDocumentService<IGlobalCaseDocumentRepository>(globalCaseDocumentRepository), IGlobalCaseDocumentService;