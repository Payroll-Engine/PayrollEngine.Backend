using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class EmployeeCaseValueSetupRepository(ICaseFieldRepository caseFieldRepository,
        IEmployeeCaseDocumentRepository caseDocumentRepository)
    : CaseValueSetupRepository(DbSchema.Tables.EmployeeCaseValue, DbSchema.EmployeeCaseValueColumn.EmployeeId,
        caseFieldRepository, caseDocumentRepository), IEmployeeCaseValueSetupRepository
{
    protected override string CaseValueTableName => DbSchema.Tables.EmployeeCaseValuePivot;
    protected override string CaseValueQueryProcedure => DbSchema.Procedures.GetEmployeeCaseValues;
}