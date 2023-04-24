using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class CompanyCaseDocumentRepository : CaseDocumentRepository, ICompanyCaseDocumentRepository
{
    public CompanyCaseDocumentRepository() :
        base(DbSchema.Tables.CompanyCaseDocument, DbSchema.CompanyCaseDocumentColumn.CaseValueId)
    {
    }
}