using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using SqlKata.Compilers;

namespace PayrollEngine.Persistence;

public abstract class RepositoryBase
{
    private readonly SqlServerCompiler compiler = new()
    {
        UseLegacyPagination = false
    };

    // Don't use the Sql or RawSql properties
    protected string CompileQuery(SqlKata.Query query) => compiler.Compile(query).ToString();

    private async Task<IEnumerable<T>> SelectByIdAsync<T>(IDbContext context, string table, int id) where T : IDomainObject =>
        await SelectAsync<T>(context, table, DbSchema.ObjectColumn.Id, id);

    protected async Task<IEnumerable<T>> SelectAsync<T>(IDbContext context, string table, string column,
        object value) where T : IDomainObject =>
        await SelectAsync<T>(context, table, new() { { column, value } });

    protected async Task<IEnumerable<T>> SelectAsync<T>(IDbContext context, string table,
        Dictionary<string, object> conditions) where T : IDomainObject
    {
        if (string.IsNullOrWhiteSpace(table))
        {
            throw new ArgumentException(nameof(table));
        }

        // query: SELECT
        var query = DbQueryFactory.NewQuery(table, conditions);
        var compileQuery = CompileQuery(query);

        // SELECT execution
        return await context.QueryAsync<T>(compileQuery);
    }

    protected async Task<T> SelectSingleByIdAsync<T>(IDbContext context, string table, int id) where T : IDomainObject =>
        (await SelectByIdAsync<T>(context, table, id)).FirstOrDefault();

    protected async Task<T> SelectSingleAsync<T>(IDbContext context, string table, string column, object value) where T : IDomainObject =>
        (await SelectAsync<T>(context, table, column, value)).FirstOrDefault();

    protected async Task<T> SelectSingleAsync<T>(IDbContext context, string table, Dictionary<string, object> conditions) where T : IDomainObject =>
        (await SelectAsync<T>(context, table, conditions)).FirstOrDefault();

    protected async Task<IEnumerable<T>> SelectInnerJoinAsync<T>(IDbContext context, string leftTable, string rightTable, string relationColumn)
    {
        if (string.IsNullOrWhiteSpace(leftTable))
        {
            throw new ArgumentException(nameof(leftTable));
        }
        if (string.IsNullOrWhiteSpace(rightTable))
        {
            throw new ArgumentException(nameof(rightTable));
        }
        if (string.IsNullOrWhiteSpace(relationColumn))
        {
            throw new ArgumentException(nameof(relationColumn));
        }

        var query = $"SELECT {rightTable}.* " +
                  $"FROM {leftTable} INNER JOIN " +
                  $"{rightTable} ON {leftTable}.{DbSchema.ObjectColumn.Id} = {rightTable}.{relationColumn}";

        // SELECT execution
        return await context.QueryAsync<T>(query);
    }
}