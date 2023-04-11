using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class GlobalCaseValueSetupRepository : CaseValueSetupRepository, IGlobalCaseValueSetupRepository
{
    public GlobalCaseValueSetupRepository(ICaseFieldRepository caseFieldRepository,
        IGlobalCaseDocumentRepository caseDocumentRepository, IDbContext context) :
        base(DbSchema.Tables.GlobalCaseValue, DbSchema.GlobalCaseValueColumn.TenantId,
            caseFieldRepository, caseDocumentRepository, context)
    {
    }
    protected override string CaseValueTableName => DbSchema.Tables.GlobalCaseValuePivot;
    protected override string CaseValueQueryProcedure => DbSchema.Procedures.GetGlobalCaseValues;
}