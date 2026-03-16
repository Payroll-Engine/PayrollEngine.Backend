using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Persistence.DbQuery;
using PayrollEngine.Persistence.DbSchema;

namespace PayrollEngine.Persistence;

public class GlobalCaseChangeRepository(CaseChangeRepositorySettings settings) : CaseChangeRepository<CaseChange>(
    Tables.GlobalCaseChange, GlobalCaseChangeColumn.TenantId, settings), IGlobalCaseChangeRepository
{
    protected override async Task<IEnumerable<CaseChangeCaseValue>> QueryCaseChangesValuesAsync(IDbContext context,
        int tenantId, int parentId, Query query = null)
    {
        // db query
        var dbQuery = DbQueryFactory.NewTypeQuery<CaseChangeCaseValue>(
            Tables.GlobalCaseChangeValuePivot, query);

        // tenant
        dbQuery.Item1.Where(GlobalCaseValueColumn.TenantId, tenantId);

        // case change query filter
        var caseChangeQuery = query as CaseChangeQuery;
        caseChangeQuery?.ApplyTo(dbQuery.Item1);

        // query compilation
        var compileQuery = CompileQuery(dbQuery.Item1, context);

        // SELECT execution
        IEnumerable<CaseChangeCaseValue> items = (await QueryCaseValuesAsync<CaseChangeCaseValue>(context,
            new()
            {
                ParentId = parentId,
                StoredProcedure = Procedures.GetGlobalCaseChangeValues,
                Query = compileQuery,
                QueryAttributes = dbQuery.Item2,
                Culture = caseChangeQuery?.Culture
            })).ToList();
        return items;
    }

    protected override async Task<long> QueryCaseChangesValuesCountAsync(IDbContext context, int tenantId, int parentId, Query query = null)
    {
        // pivot query
        var dbQuery = DbQueryFactory.NewTypeQuery<CaseChangeCaseValue>(
            Tables.GlobalCaseChangeValuePivot, query, QueryMode.ItemCount);

        // tenant
        dbQuery.Item1.Where(GlobalCaseValueColumn.TenantId, tenantId);

        // case change query filter
        var caseChangeQuery = query as CaseChangeQuery;
        caseChangeQuery?.ApplyTo(dbQuery.Item1);

        // query compilation
        var compileQuery = CompileQuery(dbQuery.Item1, context);

        // SELECT execution
        var count = await QueryCaseValueCountAsync(context,
            new()
            {
                ParentId = parentId,
                StoredProcedure = Procedures.GetGlobalCaseChangeValues,
                Query = compileQuery,
                QueryAttributes = dbQuery.Item2,
                Culture = caseChangeQuery?.Culture
            });
        return count;
    }
}