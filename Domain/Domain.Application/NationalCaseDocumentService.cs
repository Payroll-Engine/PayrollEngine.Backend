using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class NationalCaseDocumentService(INationalCaseDocumentRepository nationalCaseDocumentRepository)
    : CaseDocumentService<INationalCaseDocumentRepository>(nationalCaseDocumentRepository), INationalCaseDocumentService;