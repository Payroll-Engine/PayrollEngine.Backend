using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Persistence.DbQuery;
using PayrollEngine.Persistence.DbSchema;

namespace PayrollEngine.Persistence;

public class EmployeeCaseChangeRepository(CaseChangeRepositorySettings settings) : CaseChangeRepository<CaseChange>(
        Tables.EmployeeCaseChange, EmployeeCaseChangeColumn.EmployeeId, settings),
    IEmployeeCaseChangeRepository
{
    protected override void GetObjectCreateData(CaseChange caseChange, DbParameterCollection parameters)
    {
        parameters.Add(nameof(caseChange.EmployeeId), caseChange.EmployeeId, DbType.Int32);
        base.GetObjectCreateData(caseChange, parameters);
    }

    protected override async Task<IEnumerable<CaseChangeCaseValue>> QueryCaseChangesValuesAsync(IDbContext context,
        int tenantId, int employeeId, Query query = null)
    {
        // db query
        var dbQuery = DbQueryFactory.NewTypeQuery<CaseChangeCaseValue>(
            Tables.EmployeeCaseChangeValuePivot, query);

        // employee
        dbQuery.Item1.Where(EmployeeCaseValueColumn.EmployeeId, employeeId);

        // case change query filter
        var caseChangeQuery = query as CaseChangeQuery;
        caseChangeQuery?.ApplyTo(dbQuery.Item1);

        // query compilation
        var compileQuery = CompileQuery(dbQuery.Item1, context);

        // SELECT execution
        IEnumerable<CaseChangeCaseValue> items = (await QueryCaseValuesAsync<CaseChangeCaseValue>(context,
            new()
            {
                ParentId = employeeId,
                StoredProcedure = Procedures.GetEmployeeCaseChangeValues,
                Query = compileQuery,
                QueryAttributes = dbQuery.Item2,
                Culture = caseChangeQuery?.Culture
            })).ToList();
        return items;
    }

    protected override async Task<long> QueryCaseChangesValuesCountAsync(IDbContext context, int tenantId, int employeeId, Query query = null)
    {
        // pivot query
        var dbQuery = DbQueryFactory.NewTypeQuery<CaseChangeCaseValue>(
            Tables.EmployeeCaseChangeValuePivot, query, QueryMode.ItemCount);

        // employee
        dbQuery.Item1.Where(EmployeeCaseValueColumn.EmployeeId, employeeId);

        // case change query filter
        var caseChangeQuery = query as CaseChangeQuery;
        caseChangeQuery?.ApplyTo(dbQuery.Item1);

        // query compilation
        var compileQuery = CompileQuery(dbQuery.Item1, context);

        // SELECT execution
        var count = await QueryCaseValueCountAsync(context,
            new()
            {
                ParentId = employeeId,
                StoredProcedure = Procedures.GetEmployeeCaseChangeValues,
                Query = compileQuery,
                QueryAttributes = dbQuery.Item2,
                Culture = caseChangeQuery?.Culture
            });
        return count;
    }
}