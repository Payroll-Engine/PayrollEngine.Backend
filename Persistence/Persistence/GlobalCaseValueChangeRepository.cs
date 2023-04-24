using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class GlobalCaseValueChangeRepository : CaseValueChangeRepository, IGlobalCaseValueChangeRepository
{
    public GlobalCaseValueChangeRepository() :
        base(DbSchema.Tables.GlobalCaseValueChange, DbSchema.GlobalCaseValueChangeColumn.CaseChangeId)
    {
    }
}