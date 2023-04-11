using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using PayrollEngine.Domain.Model;
using SqlKata.Compilers;

namespace PayrollEngine.Persistence;

public abstract class RepositoryBase
{
    private readonly SqlServerCompiler compiler = new()
    {
        UseLegacyPagination = false
    };

    protected IDbContext Context { get; }
    protected IDbConnection Connection => Context.Connection;

    protected RepositoryBase(IDbContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }

    // Don't use the Sql or RawSql properties
    protected string CompileQuery(SqlKata.Query query) => compiler.Compile(query).ToString();

    protected virtual async Task<IEnumerable<T>> SelectByIdAsync<T>(string table, int id) where T : IDomainObject =>
        await SelectAsync<T>(table, DbSchema.ObjectColumn.Id, id);

    protected virtual async Task<IEnumerable<T>> SelectAsync<T>(string table, string column,
        object value) where T : IDomainObject =>
        await SelectAsync<T>(table, new() { { column, value } });

    protected virtual async Task<IEnumerable<T>> SelectAsync<T>(string table,
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
        return await Connection.QueryAsync<T>(compileQuery);
    }

    protected virtual async Task<T> SelectSingleByIdAsync<T>(string table, int id) where T : IDomainObject =>
        (await SelectByIdAsync<T>(table, id)).FirstOrDefault();

    protected virtual async Task<T> SelectSingleAsync<T>(string table, string column, object value) where T : IDomainObject =>
        (await SelectAsync<T>(table, column, value)).FirstOrDefault();

    protected virtual async Task<T> SelectSingleAsync<T>(string table, Dictionary<string, object> conditions) where T : IDomainObject =>
        (await SelectAsync<T>(table, conditions)).FirstOrDefault();

    protected virtual async Task<IEnumerable<T>> SelectInnerJoinAsync<T>(string leftTable, string rightTable, string relationColumn)
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
        return await Connection.QueryAsync<T>(query);
    }

}