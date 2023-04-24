using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class CompanyCaseValueChangeRepository : CaseValueChangeRepository, ICompanyCaseValueChangeRepository
{
    public CompanyCaseValueChangeRepository() :
        base(DbSchema.Tables.CompanyCaseValueChange, DbSchema.CompanyCaseValueChangeColumn.CaseChangeId)
    {
    }
}