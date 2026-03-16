using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Persistence.DbSchema;

namespace PayrollEngine.Persistence;

public class CompanyCaseValueSetupRepository(ICaseFieldRepository caseFieldRepository,
        ICompanyCaseDocumentRepository caseDocumentRepository)
    : CaseValueSetupRepository(Tables.CompanyCaseValue, CompanyCaseValueColumn.TenantId,
        caseFieldRepository, caseDocumentRepository), ICompanyCaseValueSetupRepository
{
    protected override string CaseValueTableName => Tables.CompanyCaseValuePivot;
    protected override string CaseValueQueryProcedure => Procedures.GetCompanyCaseValues;
}