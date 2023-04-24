using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class NationalCaseValueRepository : CaseValueRepository, INationalCaseValueRepository
{
    public NationalCaseValueRepository(ICaseFieldRepository caseFieldRepository) :
        base(DbSchema.Tables.NationalCaseValue, DbSchema.NationalCaseValueColumn.TenantId, caseFieldRepository)
    {
    }
    protected override string CaseValueTableName => DbSchema.Tables.NationalCaseValuePivot;
    protected override string CaseValueQueryProcedure => DbSchema.Procedures.GetNationalCaseValues;
}