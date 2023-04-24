using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class EmployeeCaseValueSetupRepository : CaseValueSetupRepository, IEmployeeCaseValueSetupRepository
{
    public EmployeeCaseValueSetupRepository(ICaseFieldRepository caseFieldRepository, 
        IEmployeeCaseDocumentRepository caseDocumentRepository) :
        base(DbSchema.Tables.EmployeeCaseValue, DbSchema.EmployeeCaseValueColumn.EmployeeId,
            caseFieldRepository, caseDocumentRepository)
    {
    }
    protected override string CaseValueTableName => DbSchema.Tables.EmployeeCaseValuePivot;
    protected override string CaseValueQueryProcedure => DbSchema.Procedures.GetEmployeeCaseValues;
}