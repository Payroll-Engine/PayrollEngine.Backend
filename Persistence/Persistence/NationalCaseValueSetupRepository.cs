using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Persistence.DbSchema;

namespace PayrollEngine.Persistence;

public class NationalCaseValueSetupRepository(ICaseFieldRepository caseFieldRepository,
        INationalCaseDocumentRepository caseDocumentRepository)
    : CaseValueSetupRepository(Tables.NationalCaseValue, NationalCaseValueColumn.TenantId,
        caseFieldRepository, caseDocumentRepository), INationalCaseValueSetupRepository
{
    protected override string CaseValueTableName => Tables.NationalCaseValuePivot;
    protected override string CaseValueQueryProcedure => Procedures.GetNationalCaseValues;
}