using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Persistence.DbQuery;

namespace PayrollEngine.Persistence;

public class GlobalCaseChangeRepository : CaseChangeRepository<CaseChange>, IGlobalCaseChangeRepository
{
    public GlobalCaseChangeRepository(CaseChangeRepositorySettings settings, IDbContext context) :
        base(DbSchema.Tables.GlobalCaseChange, DbSchema.GlobalCaseChangeColumn.TenantId, settings, context)
    {
    }

    protected override async Task<IEnumerable<CaseChangeCaseValue>> QueryCaseChangesValuesAsync(int tenantId, int parentId, Query query = null)
    {
        // db query
        var dbQuery = DbQueryFactory.NewTypeQuery<CaseChangeCaseValue>(
            DbSchema.Tables.GlobalCaseChangeValuePivot, query);

        // tenant
        dbQuery.Item1.Where(DbSchema.GlobalCaseValueColumn.TenantId, tenantId);

        // case change query filter
        var caseChangeQuery = query as CaseChangeQuery;
        caseChangeQuery?.ApplyTo(dbQuery.Item1);

        // query compilation
        var compileQuery = CompileQuery(dbQuery.Item1);

        // SELECT execution
        IEnumerable<CaseChangeCaseValue> items = (await QueryCaseValuesAsync<CaseChangeCaseValue>(
            new()
            {
                ParentId = parentId,
                StoredProcedure = DbSchema.Procedures.GetGlobalCaseChangeValues,
                Query = compileQuery,
                QueryAttributes = dbQuery.Item2,
                Language = caseChangeQuery?.Language
            })).ToList();
        return items;
    }

    protected override async Task<long> QueryCaseChangesValuesCountAsync(int tenantId, int parentId, Query query = null)
    {
        // pivot query
        var dbQuery = DbQueryFactory.NewTypeQuery<CaseChangeCaseValue>(
            DbSchema.Tables.GlobalCaseChangeValuePivot, query, QueryMode.ItemCount);

        // tenant
        dbQuery.Item1.Where(DbSchema.GlobalCaseValueColumn.TenantId, tenantId);

        // case change query filter
        var caseChangeQuery = query as CaseChangeQuery;
        caseChangeQuery?.ApplyTo(dbQuery.Item1);

        // query compilation
        var compileQuery = CompileQuery(dbQuery.Item1);

        // SELECT execution
        var count = await QueryCaseValueCountAsync(
            new()
            {
                ParentId = parentId,
                StoredProcedure = DbSchema.Procedures.GetGlobalCaseChangeValues,
                Query = compileQuery,
                QueryAttributes = dbQuery.Item2,
                Language = caseChangeQuery?.Language
            });
        return count;
    }
}