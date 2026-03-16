using System;
using System.Data;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Persistence.DbSchema;
using PayrollEngine.Serialization;

namespace PayrollEngine.Persistence;

public class PayrollLayerRepository() : ChildDomainRepository<PayrollLayer>(Tables.PayrollLayer,
    PayrollLayerColumn.PayrollId), IPayrollLayerRepository
{
    protected override void GetObjectData(PayrollLayer payrollLayer, DbParameterCollection parameters)
    {
        parameters.Add(nameof(payrollLayer.Level), payrollLayer.Level, DbType.Int32);
        parameters.Add(nameof(payrollLayer.Priority), payrollLayer.Priority, DbType.Int32);
        parameters.Add(nameof(payrollLayer.RegulationName), payrollLayer.RegulationName);
        parameters.Add(nameof(payrollLayer.Attributes), JsonSerializer.SerializeNamedDictionary(payrollLayer.Attributes));
        base.GetObjectData(payrollLayer, parameters);
    }

    public async Task<bool> ExistsAsync(IDbContext context, int payrollId, int level, int priority)
    {
        if (payrollId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(payrollId));
        }

        // query
        var query = DbQueryFactory.NewCountQuery(TableName)
            .Where(PayrollLayerColumn.PayrollId, payrollId)
            .Where(PayrollLayerColumn.Level, level)
            .Where(PayrollLayerColumn.Priority, priority);
        var compileQuery = CompileQuery(query, context);

        // SELECT execution
        var count = await ExecuteScalarAsync<int>(context, compileQuery);
        return count == 1;
    }
}