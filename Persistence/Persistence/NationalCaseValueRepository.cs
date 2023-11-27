using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class NationalCaseValueRepository(ICaseFieldRepository caseFieldRepository) : CaseValueRepository(
        DbSchema.Tables.NationalCaseValue, DbSchema.NationalCaseValueColumn.TenantId, caseFieldRepository),
    INationalCaseValueRepository
{
    protected override string CaseValueTableName => DbSchema.Tables.NationalCaseValuePivot;
    protected override string CaseValueQueryProcedure => DbSchema.Procedures.GetNationalCaseValues;
}