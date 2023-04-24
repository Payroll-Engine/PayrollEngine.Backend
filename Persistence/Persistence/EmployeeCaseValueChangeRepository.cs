using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class EmployeeCaseValueChangeRepository : CaseValueChangeRepository, IEmployeeCaseValueChangeRepository
{
    public EmployeeCaseValueChangeRepository() :
        base(DbSchema.Tables.EmployeeCaseValueChange, DbSchema.EmployeeCaseValueChangeColumn.CaseChangeId)
    {
    }
}