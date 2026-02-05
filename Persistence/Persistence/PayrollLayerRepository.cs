using System;
using System.Data;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Serialization;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class PayrollLayerRepository() : ChildDomainRepository<PayrollLayer>(DbSchema.Tables.PayrollLayer,
    DbSchema.PayrollLayerColumn.PayrollId), IPayrollLayerRepository
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
            .Where(DbSchema.PayrollLayerColumn.PayrollId, payrollId)
            .Where(DbSchema.PayrollLayerColumn.Level, level)
            .Where(DbSchema.PayrollLayerColumn.Priority, priority);
        var compileQuery = CompileQuery(query);

        // SELECT execution
        var count = await ExecuteScalarAsync<int>(context, compileQuery);
        return count == 1;
    }
}