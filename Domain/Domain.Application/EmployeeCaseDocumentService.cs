using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class EmployeeCaseDocumentService : CaseDocumentService<IEmployeeCaseDocumentRepository>, IEmployeeCaseDocumentService
{
    public EmployeeCaseDocumentService(IEmployeeCaseDocumentRepository employeeCaseDocumentRepository) :
        base(employeeCaseDocumentRepository)
    {
    }
}