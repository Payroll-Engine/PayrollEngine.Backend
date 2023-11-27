using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class CompanyCaseValueRepository(ICaseFieldRepository caseFieldRepository) : CaseValueRepository(
    DbSchema.Tables.CompanyCaseValue, DbSchema.CompanyCaseValueColumn.TenantId,
    caseFieldRepository), ICompanyCaseValueRepository
{
    protected override string CaseValueTableName => DbSchema.Tables.CompanyCaseValuePivot;
    protected override string CaseValueQueryProcedure => DbSchema.Procedures.GetCompanyCaseValues;
}