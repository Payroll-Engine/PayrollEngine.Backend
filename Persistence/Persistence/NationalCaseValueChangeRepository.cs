using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class NationalCaseValueChangeRepository : CaseValueChangeRepository, INationalCaseValueChangeRepository
{
    public NationalCaseValueChangeRepository() :
        base(DbSchema.Tables.NationalCaseValueChange, DbSchema.NationalCaseValueChangeColumn.CaseChangeId)
    {
    }
}