using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class CompanyCaseValueSetupRepository : CaseValueSetupRepository, ICompanyCaseValueSetupRepository
{
    public CompanyCaseValueSetupRepository(ICaseFieldRepository caseFieldRepository, 
        ICompanyCaseDocumentRepository caseDocumentRepository) :
        base(DbSchema.Tables.CompanyCaseValue, DbSchema.CompanyCaseValueColumn.TenantId,
            caseFieldRepository, caseDocumentRepository)
    {
    }
    protected override string CaseValueTableName => DbSchema.Tables.CompanyCaseValuePivot;
    protected override string CaseValueQueryProcedure => DbSchema.Procedures.GetCompanyCaseValues;
}