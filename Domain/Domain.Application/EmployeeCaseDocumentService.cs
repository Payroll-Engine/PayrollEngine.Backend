using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class EmployeeCaseDocumentService(IEmployeeCaseDocumentRepository employeeCaseDocumentRepository)
    : CaseDocumentService<IEmployeeCaseDocumentRepository>(employeeCaseDocumentRepository), IEmployeeCaseDocumentService;