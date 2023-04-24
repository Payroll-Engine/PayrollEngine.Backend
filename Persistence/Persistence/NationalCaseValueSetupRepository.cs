using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class NationalCaseValueSetupRepository : CaseValueSetupRepository, INationalCaseValueSetupRepository
{
    public NationalCaseValueSetupRepository(ICaseFieldRepository caseFieldRepository,
        INationalCaseDocumentRepository caseDocumentRepository) :
        base(DbSchema.Tables.NationalCaseValue, DbSchema.NationalCaseValueColumn.TenantId,
            caseFieldRepository, caseDocumentRepository)
    {
    }
    protected override string CaseValueTableName => DbSchema.Tables.NationalCaseValuePivot;
    protected override string CaseValueQueryProcedure => DbSchema.Procedures.GetNationalCaseValues;
}