using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Persistence.DbQuery;

namespace PayrollEngine.Persistence;

public class PayrollResultRepository : ChildDomainRepository<PayrollResult>, IPayrollResultRepository
{
    public PayrollResultRepository(IDbContext context) :
        base(DbSchema.Tables.PayrollResult, DbSchema.PayrollResultColumn.TenantId, context)
    {
    }

    #region Result Values

    /// <inheritdoc />
    public virtual async Task<IEnumerable<PayrollResultValue>> QueryResultValuesAsync(int tenantId,
        int employeeId, Query query = null)
    {
        if (tenantId <= 0)
        {
            throw new ArgumentException(nameof(tenantId));
        }

        // db query
        var dbQuery = DbQueryFactory.NewTypeQuery<PayrollResultValue>(
            DbSchema.Tables.PayrollResultPivot, query);

        // employee
        dbQuery.Item1.Where(DbSchema.PayrollResultColumn.TenantId, tenantId);

        // query compilation
        var compileQuery = CompileQuery(dbQuery.Item1);

        // SELECT execution
        IEnumerable<PayrollResultValue> items = (await QueryCaseValuesAsync<PayrollResultValue>(
            new()
            {
                ParentId = tenantId,
                EmployeeId = employeeId,
                StoredProcedure = DbSchema.Procedures.GetPayrollResultValues,
                Query = compileQuery,
                QueryAttributes = dbQuery.Item2
            })).ToList();
        return items;
    }

    /// <inheritdoc />
    public virtual async Task<long> QueryResultValueCountAsync(int tenantId, int employeeId, Query query = null)
    {
        if (tenantId <= 0)
        {
            throw new ArgumentException(nameof(tenantId));
        }

        // db query
        var dbQuery = DbQueryFactory.NewTypeQuery<PayrollResultValue>(
            DbSchema.Tables.PayrollResultPivot, query, QueryMode.ItemCount);

        // employee
        dbQuery.Item1.Where(DbSchema.PayrollResultColumn.TenantId, tenantId);

        // query compilation
        var compileQuery = CompileQuery(dbQuery.Item1);

        // SELECT execution
        var count = await QueryCaseValueCountAsync(
            new()
            {
                ParentId = tenantId,
                EmployeeId = employeeId,
                StoredProcedure = DbSchema.Procedures.GetPayrollResultValues,
                Query = compileQuery,
                QueryAttributes = dbQuery.Item2
            });
        return count;
    }

    #endregion

    #region Wage Type results

    /// <inheritdoc />
    public virtual async Task<IEnumerable<WageTypeResult>> GetWageTypeResultsAsync(WageTypeResultQuery query,
        int? payrunJobId = null, int? parentPayrunJobId = null) =>
        await new WageTypeResultCommand(Connection).GetResultsAsync(query, payrunJobId, parentPayrunJobId);

    /// <inheritdoc />
    public virtual async Task<IEnumerable<WageTypeCustomResult>> GetWageTypeCustomResultsAsync(WageTypeResultQuery query,
        int? payrunJobId = null, int? parentPayrunJobId = null) =>
        await new WageTypeCustomResultCommand(Connection).GetResultsAsync(query, payrunJobId, parentPayrunJobId);

    #endregion

    #region Collector results

    /// <inheritdoc />
    public virtual async Task<IEnumerable<CollectorResult>> GetCollectorResultsAsync(CollectorResultQuery query,
        int? payrunJobId = null, int? parentPayrunJobId = null) =>
        await new CollectorResultCommand(Connection).GetResultsAsync(query, payrunJobId, parentPayrunJobId);

    /// <inheritdoc />
    public virtual async Task<IEnumerable<CollectorCustomResult>> GetCollectorCustomResultsAsync(CollectorResultQuery query,
        int? payrunJobId = null, int? parentPayrunJobId = null) =>
        await new CollectorCustomResultCommand(Connection).GetResultsAsync(query, payrunJobId, parentPayrunJobId);

    #endregion

}