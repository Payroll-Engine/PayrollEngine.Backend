using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class GlobalCaseDocumentRepository : CaseDocumentRepository, IGlobalCaseDocumentRepository
{
    public GlobalCaseDocumentRepository(IDbContext context) :
        base(DbSchema.Tables.GlobalCaseDocument, DbSchema.GlobalCaseDocumentColumn.CaseValueId, context)
    {
    }
}