using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class NationalCaseDocumentRepository : CaseDocumentRepository, INationalCaseDocumentRepository
{
    public NationalCaseDocumentRepository() : 
        base(DbSchema.Tables.NationalCaseDocument, DbSchema.NationalCaseDocumentColumn.CaseValueId)
    {
    }
}