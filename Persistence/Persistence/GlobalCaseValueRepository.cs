using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class GlobalCaseValueRepository(ICaseFieldRepository caseFieldRepository) : CaseValueRepository(
        DbSchema.Tables.GlobalCaseValue, DbSchema.GlobalCaseValueColumn.TenantId, caseFieldRepository),
    IGlobalCaseValueRepository
{
    protected override string CaseValueTableName => DbSchema.Tables.GlobalCaseValuePivot;
    protected override string CaseValueQueryProcedure => DbSchema.Procedures.GetGlobalCaseValues;
}