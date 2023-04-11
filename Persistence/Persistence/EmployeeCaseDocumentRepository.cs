using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class EmployeeCaseDocumentRepository : CaseDocumentRepository, IEmployeeCaseDocumentRepository
{
    public EmployeeCaseDocumentRepository(IDbContext context) :
        base(DbSchema.Tables.EmployeeCaseDocument, DbSchema.EmployeeCaseDocumentColumn.CaseValueId, context)
    {
    }
}