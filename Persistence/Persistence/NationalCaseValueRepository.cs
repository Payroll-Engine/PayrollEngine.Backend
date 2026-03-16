using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Persistence.DbSchema;

namespace PayrollEngine.Persistence;

public class NationalCaseValueRepository(ICaseFieldRepository caseFieldRepository) : CaseValueRepository(
        Tables.NationalCaseValue, NationalCaseValueColumn.TenantId, caseFieldRepository),
    INationalCaseValueRepository
{
    protected override string CaseValueTableName => Tables.NationalCaseValuePivot;
    protected override string CaseValueQueryProcedure => Procedures.GetNationalCaseValues;
}