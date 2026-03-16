using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Persistence.DbSchema;
using PayrollEngine.Serialization;

namespace PayrollEngine.Persistence;

public class EmployeeCaseValueRepository(ICaseFieldRepository caseFieldRepository) : CaseValueRepository(
    Tables.EmployeeCaseValue, EmployeeCaseValueColumn.EmployeeId,
    caseFieldRepository), IEmployeeCaseValueRepository
{
    protected override string CaseValueTableName => Tables.EmployeeCaseValuePivot;
    protected override string CaseValueQueryProcedure => Procedures.GetEmployeeCaseValues;

    /// <inheritdoc />
    public async Task<IEnumerable<CaseValue>> GetTenantCaseValuesAsync(IDbContext context, int tenantId,
        DateTime? valueDate = null, DateTime? evaluationDate = null,
        IEnumerable<string> caseFieldNames = null, string forecast = null)
    {
        if (tenantId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tenantId));
        }

        // serialize field names as JSON array for SP parameter
        string fieldNamesJson = null;
        if (caseFieldNames != null)
        {
            fieldNamesJson = JsonSerializer.SerializeList(new List<string>(caseFieldNames));
        }

        return await QueryAsync<CaseValue>(context,
            Procedures.GetEmployeeCaseValuesByTenant,
            new
            {
                tenantId,
                valueDate = valueDate?.ToUtc(),
                evaluationDate = evaluationDate?.ToUtc(),
                fieldNames = fieldNamesJson,
                forecast
            },
            commandType: CommandType.StoredProcedure);
    }
}
