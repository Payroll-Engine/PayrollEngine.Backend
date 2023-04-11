using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class GlobalCaseValueRepository : CaseValueRepository, IGlobalCaseValueRepository
{
    public GlobalCaseValueRepository(ICaseFieldRepository caseFieldRepository, IDbContext context) :
        base(DbSchema.Tables.GlobalCaseValue, DbSchema.GlobalCaseValueColumn.TenantId,
            caseFieldRepository, context)
    {
    }
    protected override string CaseValueTableName => DbSchema.Tables.GlobalCaseValuePivot;
    protected override string CaseValueQueryProcedure => DbSchema.Procedures.GetGlobalCaseValues;
}