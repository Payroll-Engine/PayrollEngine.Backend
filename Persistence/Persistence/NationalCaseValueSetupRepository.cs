using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class NationalCaseValueSetupRepository(ICaseFieldRepository caseFieldRepository,
        INationalCaseDocumentRepository caseDocumentRepository)
    : CaseValueSetupRepository(DbSchema.Tables.NationalCaseValue, DbSchema.NationalCaseValueColumn.TenantId,
        caseFieldRepository, caseDocumentRepository), INationalCaseValueSetupRepository
{
    protected override string CaseValueTableName => DbSchema.Tables.NationalCaseValuePivot;
    protected override string CaseValueQueryProcedure => DbSchema.Procedures.GetNationalCaseValues;
}