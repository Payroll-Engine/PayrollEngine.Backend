using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class GlobalCaseValueSetupRepository(ICaseFieldRepository caseFieldRepository,
        IGlobalCaseDocumentRepository caseDocumentRepository)
    : CaseValueSetupRepository(DbSchema.Tables.GlobalCaseValue, DbSchema.GlobalCaseValueColumn.TenantId,
        caseFieldRepository, caseDocumentRepository), IGlobalCaseValueSetupRepository
{
    protected override string CaseValueTableName => DbSchema.Tables.GlobalCaseValuePivot;
    protected override string CaseValueQueryProcedure => DbSchema.Procedures.GetGlobalCaseValues;
}