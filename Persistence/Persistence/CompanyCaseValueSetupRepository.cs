using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class CompanyCaseValueSetupRepository(ICaseFieldRepository caseFieldRepository,
        ICompanyCaseDocumentRepository caseDocumentRepository)
    : CaseValueSetupRepository(DbSchema.Tables.CompanyCaseValue, DbSchema.CompanyCaseValueColumn.TenantId,
        caseFieldRepository, caseDocumentRepository), ICompanyCaseValueSetupRepository
{
    protected override string CaseValueTableName => DbSchema.Tables.CompanyCaseValuePivot;
    protected override string CaseValueQueryProcedure => DbSchema.Procedures.GetCompanyCaseValues;
}