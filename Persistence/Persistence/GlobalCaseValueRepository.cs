using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class GlobalCaseValueRepository : CaseValueRepository, IGlobalCaseValueRepository
{
    public GlobalCaseValueRepository(ICaseFieldRepository caseFieldRepository) :
        base(DbSchema.Tables.GlobalCaseValue, DbSchema.GlobalCaseValueColumn.TenantId, caseFieldRepository)
    {
    }
    protected override string CaseValueTableName => DbSchema.Tables.GlobalCaseValuePivot;
    protected override string CaseValueQueryProcedure => DbSchema.Procedures.GetGlobalCaseValues;
}