using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Persistence.DbSchema;

namespace PayrollEngine.Persistence;

public class CompanyCaseValueRepository(ICaseFieldRepository caseFieldRepository) : CaseValueRepository(
    Tables.CompanyCaseValue, CompanyCaseValueColumn.TenantId,
    caseFieldRepository), ICompanyCaseValueRepository
{
    protected override string CaseValueTableName => Tables.CompanyCaseValuePivot;
    protected override string CaseValueQueryProcedure => Procedures.GetCompanyCaseValues;
}