using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class GlobalCaseValueChangeRepository : CaseValueChangeRepository, IGlobalCaseValueChangeRepository
{
    public GlobalCaseValueChangeRepository(IDbContext context) :
        base(DbSchema.Tables.GlobalCaseValueChange, DbSchema.GlobalCaseValueChangeColumn.CaseChangeId, context)
    {
    }
}