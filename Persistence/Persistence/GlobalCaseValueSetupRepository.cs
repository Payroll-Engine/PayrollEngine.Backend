using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Persistence.DbSchema;

namespace PayrollEngine.Persistence;

public class GlobalCaseValueSetupRepository(ICaseFieldRepository caseFieldRepository,
        IGlobalCaseDocumentRepository caseDocumentRepository)
    : CaseValueSetupRepository(Tables.GlobalCaseValue, GlobalCaseValueColumn.TenantId,
        caseFieldRepository, caseDocumentRepository), IGlobalCaseValueSetupRepository
{
    protected override string CaseValueTableName => Tables.GlobalCaseValuePivot;
    protected override string CaseValueQueryProcedure => Procedures.GetGlobalCaseValues;
}