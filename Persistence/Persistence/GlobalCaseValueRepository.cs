using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Persistence.DbSchema;

namespace PayrollEngine.Persistence;

public class GlobalCaseValueRepository(ICaseFieldRepository caseFieldRepository) : CaseValueRepository(
        Tables.GlobalCaseValue, GlobalCaseValueColumn.TenantId, caseFieldRepository),
    IGlobalCaseValueRepository
{
    protected override string CaseValueTableName => Tables.GlobalCaseValuePivot;
    protected override string CaseValueQueryProcedure => Procedures.GetGlobalCaseValues;
}