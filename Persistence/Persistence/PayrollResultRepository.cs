using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Persistence.DbQuery;
using PayrollEngine.Persistence.DbSchema;

namespace PayrollEngine.Persistence;

public class PayrollResultRepository() : ChildDomainRepository<PayrollResult>(Tables.PayrollResult,
    PayrollResultColumn.TenantId), IPayrollResultRepository
{
    #region Result Values

    /// <inheritdoc />
    public async Task<IEnumerable<PayrollResultValue>> QueryResultValuesAsync(IDbContext context,
        int tenantId, int? employeeId = null, Query query = null)
    {
        if (tenantId <= 0)
        {
            throw new ArgumentException(nameof(tenantId));
        }

        // db query
        var dbQuery = DbQueryFactory.NewTypeQuery<PayrollResultValue>(
            Tables.PayrollResultPivot, query);

        // employee
        dbQuery.Item1.Where(PayrollResultColumn.TenantId, tenantId);

        // query compilation
        var compileQuery = CompileQuery(dbQuery.Item1, context);

        // SELECT execution
        IEnumerable<PayrollResultValue> items = (await QueryCaseValuesAsync<PayrollResultValue>(context,
            new()
            {
                ParentId = tenantId,
                EmployeeId = employeeId,
                StoredProcedure = Procedures.GetPayrollResultValues,
                Query = compileQuery,
                QueryAttributes = dbQuery.Item2
            })).ToList();
        return items;
    }

    /// <inheritdoc />
    public async Task<long> QueryResultValueCountAsync(IDbContext context, int tenantId, int? employeeId = null, Query query = null)
    {
        if (tenantId <= 0)
        {
            throw new ArgumentException(nameof(tenantId));
        }

        // db query
        var dbQuery = DbQueryFactory.NewTypeQuery<PayrollResultValue>(
            Tables.PayrollResultPivot, query, QueryMode.ItemCount);

        // employee
        dbQuery.Item1.Where(PayrollResultColumn.TenantId, tenantId);

        // query compilation
        var compileQuery = CompileQuery(dbQuery.Item1, context);

        // SELECT execution
        var count = await QueryCaseValueCountAsync(context,
            new()
            {
                ParentId = tenantId,
                EmployeeId = employeeId,
                StoredProcedure = Procedures.GetPayrollResultValues,
                Query = compileQuery,
                QueryAttributes = dbQuery.Item2
            });
        return count;
    }

    #endregion

    #region Wage Type results

    /// <inheritdoc />
    public async Task<IEnumerable<WageTypeResult>> GetWageTypeResultsAsync(IDbContext context,
        WageTypeResultQuery query, int? payrunJobId = null, int? parentPayrunJobId = null) =>
        await new WageTypeResultCommand(context).GetResultsAsync(query, payrunJobId, parentPayrunJobId);

    /// <inheritdoc />
    public async Task<IEnumerable<WageTypeCustomResult>> GetWageTypeCustomResultsAsync(IDbContext context,
        WageTypeResultQuery query, int? payrunJobId = null, int? parentPayrunJobId = null) =>
        await new WageTypeCustomResultCommand(context).GetResultsAsync(query, payrunJobId, parentPayrunJobId);

    #endregion

    #region Collector results

    /// <inheritdoc />
    public async Task<IEnumerable<CollectorResult>> GetCollectorResultsAsync(IDbContext context,
        CollectorResultQuery query, int? payrunJobId = null, int? parentPayrunJobId = null) =>
        await new CollectorResultCommand(context).GetResultsAsync(query, payrunJobId, parentPayrunJobId);

    /// <inheritdoc />
    public async Task<IEnumerable<CollectorCustomResult>> GetCollectorCustomResultsAsync(IDbContext context,
    CollectorResultQuery query, int? payrunJobId = null, int? parentPayrunJobId = null) =>
        await new CollectorCustomResultCommand(context).GetResultsAsync(query, payrunJobId, parentPayrunJobId);

    #endregion

}