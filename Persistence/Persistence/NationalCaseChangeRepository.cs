using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Persistence.DbQuery;

namespace PayrollEngine.Persistence;

public class NationalCaseChangeRepository : CaseChangeRepository<CaseChange>, INationalCaseChangeRepository
{
    public NationalCaseChangeRepository(CaseChangeRepositorySettings settings) :
        base(DbSchema.Tables.NationalCaseChange, DbSchema.NationalCaseChangeColumn.TenantId, settings)
    {
    }

    protected override async Task<IEnumerable<CaseChangeCaseValue>> QueryCaseChangesValuesAsync(IDbContext context,
        int tenantId, int parentId, Query query = null)
    {
        // db query
        var dbQuery = DbQueryFactory.NewTypeQuery<CaseChangeCaseValue>(
            DbSchema.Tables.NationalCaseChangeValuePivot, query);
        // tenant
        dbQuery.Item1.Where(DbSchema.NationalCaseValueColumn.TenantId, tenantId);

        // case change query filter
        var caseChangeQuery = query as CaseChangeQuery;
        caseChangeQuery?.ApplyTo(dbQuery.Item1);

        // query compilation
        var compileQuery = CompileQuery(dbQuery.Item1);

        // SELECT execution
        IEnumerable<CaseChangeCaseValue> items = (await QueryCaseValuesAsync<CaseChangeCaseValue>(context,
            new()
            {
                ParentId = parentId,
                StoredProcedure = DbSchema.Procedures.GetNationalCaseChangeValues,
                Query = compileQuery,
                QueryAttributes = dbQuery.Item2,
                Language = caseChangeQuery?.Language
            })).ToList();
        return items;
    }

    protected override async Task<long> QueryCaseChangesValuesCountAsync(IDbContext context, int tenantId, int parentId, Query query = null)
    {
        // pivot query
        var dbQuery = DbQueryFactory.NewTypeQuery<CaseChangeCaseValue>(
            DbSchema.Tables.NationalCaseChangeValuePivot, query, QueryMode.ItemCount);

        // tenant
        dbQuery.Item1.Where(DbSchema.NationalCaseValueColumn.TenantId, tenantId);

        // case change query filter
        var caseChangeQuery = query as CaseChangeQuery;
        caseChangeQuery?.ApplyTo(dbQuery.Item1);

        // query compilation
        var compileQuery = CompileQuery(dbQuery.Item1);

        // SELECT execution
        var count = await QueryCaseValueCountAsync(context,
            new()
            {
                ParentId = parentId,
                StoredProcedure = DbSchema.Procedures.GetNationalCaseChangeValues,
                Query = compileQuery,
                QueryAttributes = dbQuery.Item2,
                Language = caseChangeQuery?.Language
            });
        return count;
    }
}