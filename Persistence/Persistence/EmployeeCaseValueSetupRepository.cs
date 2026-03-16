using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Persistence.DbSchema;

namespace PayrollEngine.Persistence;

public class EmployeeCaseValueSetupRepository(ICaseFieldRepository caseFieldRepository,
        IEmployeeCaseDocumentRepository caseDocumentRepository)
    : CaseValueSetupRepository(Tables.EmployeeCaseValue, EmployeeCaseValueColumn.EmployeeId,
        caseFieldRepository, caseDocumentRepository), IEmployeeCaseValueSetupRepository
{
    protected override string CaseValueTableName => Tables.EmployeeCaseValuePivot;
    protected override string CaseValueQueryProcedure => Procedures.GetEmployeeCaseValues;
}