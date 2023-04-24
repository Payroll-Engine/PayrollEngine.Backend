using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class EmployeeCaseDocumentRepository : CaseDocumentRepository, IEmployeeCaseDocumentRepository
{
    public EmployeeCaseDocumentRepository() :
        base(DbSchema.Tables.EmployeeCaseDocument, DbSchema.EmployeeCaseDocumentColumn.CaseValueId)
    {
    }
}