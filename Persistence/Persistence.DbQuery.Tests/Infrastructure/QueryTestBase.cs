using SqlKata.Compilers;
using SqlKata;

// Alias required: the test project namespace starts with PayrollEngine.Persistence.DbQuery,
// which shadows the DbQuery namespace from the Persistence project in some contexts.
using DomainQuery = PayrollEngine.Query;

namespace PayrollEngine.Persistence.DbQuery.Tests.Infrastructure;

/// <summary>
/// Base class for all OData/DbQuery tests.
///
/// SqlKata parametrizes all literal values (= @p0, > @p0 etc.).
/// Use Sql() for structural assertions (WHERE, ORDER BY, column names).
/// Use Result() for value assertions via .Bindings.
/// </summary>
public abstract class QueryTestBase
{
    protected const string TableName = "TestTable";

    protected static readonly SqlServerCompiler Compiler = new();

    // -------------------------------------------------------------------------
    // Core helpers
    // -------------------------------------------------------------------------

    /// <summary>Returns the compiled SQL string (column names, clauses — no literal values).</summary>
    protected static string Sql(
        string filter = null,
        string orderBy = null,
        string select = null,
        int? top = null,
        int? skip = null,
        QueryMode queryMode = QueryMode.Item,
        ObjectStatus? status = null,
        string apply = null)
    {
        var sqlQuery = BuildSqlQuery<TestEntity>(null!, filter, orderBy, select, top, skip, queryMode, status, apply);
        return Compiler.Compile(sqlQuery).Sql;
    }

    /// <summary>Returns the full SqlResult including Bindings for value assertions.</summary>
    protected static SqlResult Result(
        string filter = null,
        string orderBy = null,
        string select = null,
        int? top = null,
        int? skip = null,
        QueryMode queryMode = QueryMode.Item,
        ObjectStatus? status = null,
        string apply = null)
    {
        var sqlQuery = BuildSqlQuery<TestEntity>(null!, filter, orderBy, select, top, skip, queryMode, status, apply);
        return Compiler.Compile(sqlQuery);
    }

    /// <summary>Compile a typed query for a custom entity type.</summary>
    protected static string Sql<T>(
        StubDbContext dbContext,
        string filter = null,
        string orderBy = null,
        string select = null,
        int? top = null,
        int? skip = null,
        QueryMode queryMode = QueryMode.Item)
    {
        var sqlQuery = BuildSqlQuery<T>(dbContext, filter, orderBy, select, top, skip, queryMode);
        return Compiler.Compile(sqlQuery).Sql;
    }

    /// <summary>Compile a typed query for a custom entity type — returns full result.</summary>
    protected static SqlResult Result<T>(
        StubDbContext dbContext,
        string filter = null,
        string orderBy = null,
        string select = null,
        int? top = null,
        int? skip = null,
        QueryMode queryMode = QueryMode.Item)
    {
        var sqlQuery = BuildSqlQuery<T>(dbContext, filter, orderBy, select, top, skip, queryMode);
        return Compiler.Compile(sqlQuery);
    }

    // -------------------------------------------------------------------------
    // Internal
    // -------------------------------------------------------------------------

    private static SqlKata.Query BuildSqlQuery<T>(
        Domain.Model.IDbContext dbContext,
        string filter, string orderBy, string select, int? top, int? skip,
        QueryMode queryMode, ObjectStatus? status = null, string apply = null)
    {
        var query = new DomainQuery
        {
            Filter = filter,
            OrderBy = orderBy,
            Select = select,
            Top = top,
            Skip = skip,
            Status = status,
            Apply = apply
        };
        return DbQueryFactory.NewQuery<T>(
            dbContext, TableName, query, queryMode);
    }
}
