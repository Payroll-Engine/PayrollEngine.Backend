using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class CompanyCaseValueRepository : CaseValueRepository, ICompanyCaseValueRepository
{
    public CompanyCaseValueRepository(ICaseFieldRepository caseFieldRepository, IDbContext context) :
        base(DbSchema.Tables.CompanyCaseValue, DbSchema.CompanyCaseValueColumn.TenantId,
            caseFieldRepository, context)
    {
    }
    protected override string CaseValueTableName => DbSchema.Tables.CompanyCaseValuePivot;
    protected override string CaseValueQueryProcedure => DbSchema.Procedures.GetCompanyCaseValues;
}