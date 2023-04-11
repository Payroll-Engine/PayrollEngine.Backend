using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class EmployeeCaseValueRepository : CaseValueRepository, IEmployeeCaseValueRepository
{
    public EmployeeCaseValueRepository(ICaseFieldRepository caseFieldRepository, IDbContext context) :
        base(DbSchema.Tables.EmployeeCaseValue, DbSchema.EmployeeCaseValueColumn.EmployeeId,
            caseFieldRepository, context)
    {
    }
    protected override string CaseValueTableName => DbSchema.Tables.EmployeeCaseValuePivot;
    protected override string CaseValueQueryProcedure => DbSchema.Procedures.GetEmployeeCaseValues;
}