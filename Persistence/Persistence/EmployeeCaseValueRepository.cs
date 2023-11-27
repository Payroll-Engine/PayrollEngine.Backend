using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class EmployeeCaseValueRepository(ICaseFieldRepository caseFieldRepository) : CaseValueRepository(
    DbSchema.Tables.EmployeeCaseValue, DbSchema.EmployeeCaseValueColumn.EmployeeId,
    caseFieldRepository), IEmployeeCaseValueRepository
{
    protected override string CaseValueTableName => DbSchema.Tables.EmployeeCaseValuePivot;
    protected override string CaseValueQueryProcedure => DbSchema.Procedures.GetEmployeeCaseValues;
}