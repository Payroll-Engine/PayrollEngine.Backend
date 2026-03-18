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

        // Use DbParameterCollection so MySQL DbContext.MapSpParameters
        // can convert PascalCase names to p_camelCase for MySQL SPs.
        var parameters = new DbParameterCollection();
        parameters.Add(nameof(tenantId), tenantId, DbType.Int32);
        parameters.Add(nameof(valueDate), valueDate?.ToUtc(), DbType.DateTime2);
        parameters.Add(nameof(evaluationDate), evaluationDate?.ToUtc(), DbType.DateTime2);
        parameters.Add("fieldNames", fieldNamesJson, DbType.String);
        parameters.Add(nameof(forecast), forecast, DbType.String);

        return await QueryAsync<CaseValue>(context,
            Procedures.GetEmployeeCaseValuesByTenant,
            parameters,
            commandType: CommandType.StoredProcedure);
    }
}
