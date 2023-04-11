using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class EmployeeCaseValueChangeRepository : CaseValueChangeRepository, IEmployeeCaseValueChangeRepository
{
    public EmployeeCaseValueChangeRepository(IDbContext context) :
        base(DbSchema.Tables.EmployeeCaseValueChange, DbSchema.EmployeeCaseValueChangeColumn.CaseChangeId, context)
    {
    }
}