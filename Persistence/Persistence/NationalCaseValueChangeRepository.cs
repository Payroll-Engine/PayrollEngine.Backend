using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class NationalCaseValueChangeRepository : CaseValueChangeRepository, INationalCaseValueChangeRepository
{
    public NationalCaseValueChangeRepository(IDbContext context) :
        base(DbSchema.Tables.NationalCaseValueChange, DbSchema.NationalCaseValueChangeColumn.CaseChangeId, context)
    {
    }
}