using System;
using System.Collections.Generic;
using System.Linq;
using PayrollEngine.Domain.Model;
using PayrollEngine.Persistence.DbQuery;
using PayrollEngine.Persistence.DbSchema;
using SqlKata;

namespace PayrollEngine.Persistence;

public static class DbQueryFactory
{

    #region Basic query

    /// <summary>
    /// New query with table name
    /// </summary>
    /// <param name="tableName">The table name</param>
    /// <returns>The query</returns>
    public static SqlKata.Query NewQuery(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException(nameof(tableName));
        }
        return new(tableName);
    }

    /// <summary>
    /// Builds a SQL query via the given query builder.
    /// The optional <paramref name="dbContext"/> is forwarded to the builder so that
    /// <see cref="FilterClauseBuilder"/> can generate db-specific FROM fragments for any() sub-queries.
    /// </summary>
    private static SqlKata.Query BuildQuery(QueryBuilderBase queryBuilder, string tableName,
        Query query = null, QueryMode queryMode = QueryMode.Item, IDbContext dbContext = null)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException(nameof(tableName));
        }

        // provide the db context so FilterClauseBuilder can emit backend-specific SQL for any()
        queryBuilder.DbContext = dbContext;

        // build query including OData support
        var dbQuery = queryBuilder.BuildQuery(tableName, query, queryMode);

        // object status
        dbQuery
            .When(query?.Status == ObjectStatus.Active,
                q => q.Where(ObjectColumn.Status, (int)ObjectStatus.Active))
            .When(query?.Status == ObjectStatus.Inactive,
                q => q.Where(ObjectColumn.Status, (int)ObjectStatus.Inactive));

        return dbQuery;
    }

    /// <summary>
    /// New query with multiple where conditions
    /// </summary>
    public static SqlKata.Query NewQuery(string tableName, IReadOnlyDictionary<string, object> conditions) =>
        NewQuery(tableName).Where(conditions);

    /// <summary>
    /// New query with table name and a where condition
    /// </summary>
    public static SqlKata.Query NewQuery(string tableName, string parentColumn, object parentId)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException(nameof(tableName));
        }
        if (string.IsNullOrWhiteSpace(parentColumn))
        {
            throw new ArgumentException(nameof(parentColumn));
        }

        // query
        return NewQuery(tableName)
            // parent relation
            .Where(parentColumn, parentId);
    }

    /// <summary>
    /// New query with table name and an object id condition
    /// </summary>
    public static SqlKata.Query NewQuery(string tableName, int objectId) =>
        NewQuery(tableName, ObjectColumn.Id, objectId);

    #endregion

    #region Type query

    /// <summary>
    /// New type query
    /// </summary>
    /// <param name="dbContext">The database context</param>
    /// <param name="tableName">The table name</param>
    /// <param name="query">The query</param>
    /// <param name="queryMode">The query mode</param>
    /// <returns>The query</returns>
    public static SqlKata.Query NewQuery<T>(IDbContext dbContext, string tableName, Query query = null,
        QueryMode queryMode = QueryMode.Item)
    {
        if (typeof(T).GetInterface(nameof(IDomainAttributeObject)) != null)
        {
            var dynamicQueryBuilder = new DynamicTypeQueryBuilder<T>(Prefixes.AttributePrefixes);
            var dbQuery = BuildQuery(dynamicQueryBuilder, tableName, query, queryMode, dbContext);

            // no attribute query
            if (!dynamicQueryBuilder.DynamicColumns.Any())
            {
                return dbQuery;
            }

            var dynamicColumn = false;
            foreach (var column in dynamicQueryBuilder.DynamicColumns)
            {
                if (column.IsTextAttributeField())
                {
                    dynamicColumn = true;
                }
                else if (column.IsDateAttributeField())
                {
                    dynamicColumn = true;
                }
                else if (column.IsNumericAttributeField())
                {
                    dynamicColumn = true;
                }
            }

            // ensure * is added FIRST so MySQL allows SELECT *, expr FROM table
            if (dynamicColumn && !dbQuery.Clauses.Any(x => x is Column))
            {
                dbQuery.Select("*");
            }

            // add attribute expressions AFTER *
            foreach (var column in dynamicQueryBuilder.DynamicColumns)
            {
                if (column.IsTextAttributeField())
                {
                    dbQuery.SelectRaw(dbContext.BuildAttributeQuery(column));
                }
                else if (column.IsDateAttributeField())
                {
                    dbQuery.SelectRaw(dbContext.BuildAttributeQuery(column, dbContext.DateTimeType));
                }
                else if (column.IsNumericAttributeField())
                {
                    dbQuery.SelectRaw(dbContext.BuildAttributeQuery(column, dbContext.DecimalType));
                }
            }

            // dynamic query
            var dynamicQuery = new SqlKata.Query();

            // transfer parent clauses
            var dynamicClauses = dbQuery.Clauses.
                Where(x => x is AbstractCondition or AbstractOrderBy or
                           LimitClause or OffsetClause or AggregateClause).ToList();
            foreach (var dynamicClause in dynamicClauses)
            {
                dynamicQuery.Clauses.Add(dynamicClause);
                dbQuery.Clauses.Remove(dynamicClause);
            }

            // count query
            if (queryMode == QueryMode.ItemCount)
            {
                var countQuery = new SqlKata.Query();

                // transfer aggregates to count query
                var countClauses = dynamicQuery.Clauses.
                    Where(x => x is AggregateClause).ToList();
                foreach (var countClause in countClauses)
                {
                    countQuery.Clauses.Add(countClause);
                    dynamicQuery.Clauses.Remove(countClause);
                }

                // select dynamic
                dynamicQuery.From(dbQuery, $"Dynamic{tableName}");
                // select count
                countQuery.From(dynamicQuery, $"Count{tableName}");
                return countQuery;
            }

            // select dynamic
            dynamicQuery.From(dbQuery, $"Dynamic{tableName}");
            return dynamicQuery;
        }

        var queryBuilder = new TypeQueryBuilder<T>();
        return BuildQuery(queryBuilder, tableName, query, queryMode, dbContext);
    }

    /// <summary>
    /// New type query with parent
    /// </summary>
    /// <param name="dbContext">The database context</param>
    /// <param name="tableName">The table name</param>
    /// <param name="parentColumn"></param>
    /// <param name="parentId"></param>
    /// <param name="query">The query</param>
    /// <param name="queryMode">The query mode</param>
    /// <returns>The query</returns>
    public static SqlKata.Query NewQuery<T>(IDbContext dbContext, string tableName, string parentColumn, int parentId,
        Query query = null, QueryMode queryMode = QueryMode.Item)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException(nameof(tableName));
        }
        if (string.IsNullOrWhiteSpace(parentColumn))
        {
            throw new ArgumentException(nameof(parentColumn));
        }
        if (parentId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(parentId));
        }

        // base query
        var dbQuery = NewQuery<T>(dbContext, tableName, query, queryMode);

        // parent filter
        dbQuery.Where(parentColumn, parentId);

        return dbQuery;
    }

    #endregion

    #region Dynamic Type query

    /// <summary>
    /// New type query
    /// </summary>
    /// <param name="tableName">The table name</param>
    /// <param name="query">The query</param>
    /// <param name="queryMode">The query mode</param>
    /// <returns>The query and the requested dynamic columns</returns>
    public static Tuple<SqlKata.Query, List<string>> NewTypeQuery<T>(string tableName, Query query = null,
        QueryMode queryMode = QueryMode.Item)
    {
        var queryBuilder = new DynamicTypeQueryBuilder<T>(Prefixes.AttributePrefixes);
        var dbQuery = BuildQuery(queryBuilder, tableName, query, queryMode);
        return new(dbQuery, queryBuilder.DynamicColumns);
    }

    /// <summary>
    /// New type query with parent
    /// </summary>
    /// <param name="tableName">The table name</param>
    /// <param name="parentColumn"></param>
    /// <param name="parentId"></param>
    /// <param name="query">The query</param>
    /// <param name="queryMode">The query mode</param>
    /// <returns>The query</returns>
    public static Tuple<SqlKata.Query, List<string>> NewTypeQuery<T>(string tableName, string parentColumn, int parentId,
        Query query = null, QueryMode queryMode = QueryMode.Item)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException(nameof(tableName));
        }
        if (string.IsNullOrWhiteSpace(parentColumn))
        {
            throw new ArgumentException(nameof(parentColumn));
        }
        if (parentId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(parentId));
        }

        // build base query
        var dbQuery = NewTypeQuery<T>(tableName, query, queryMode);

        // set the parent filter
        dbQuery.Item1.Where(parentColumn, parentId);

        return dbQuery;
    }

    #endregion

    #region Create

    /// <summary>
    /// New bulk insert query
    /// </summary>
    public static string NewBulkInsertQuery(string table, IEnumerable<string> columns)
    {
        if (string.IsNullOrWhiteSpace(table))
        {
            throw new ArgumentException(nameof(table));
        }
        if (columns == null)
        {
            throw new ArgumentException(nameof(columns));
        }
        var columnList = columns.ToList();
        if (!columnList.Any())
        {
            throw new ArgumentException(nameof(columns));
        }

        return $"INSERT INTO {table} (" +
               $"{string.Join(',', columnList.Select(x => $"[{x}]"))}" +
               ") VALUES (" +
               $"{string.Join(',', columnList.Select(x => $"@{x}"))})";
    }

    #endregion

    #region Count

    /// <summary>
    /// New count query with table name
    /// </summary>
    public static SqlKata.Query NewCountQuery(string table) =>
        NewQuery(table).AsCount();

    /// <summary>
    /// New count query with table name and object id condition
    /// </summary>
    public static SqlKata.Query NewCountQuery(string table, int objectId)
    {
        if (objectId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(objectId));
        }
        return NewCountQuery(table, ObjectColumn.Id, objectId);
    }

    /// <summary>
    /// New count query with table name and with field condition
    /// </summary>
    public static SqlKata.Query NewCountQuery(string table, string column, object value) =>
        NewCountQuery(table).Where(column, value);

    #endregion

    #region Delete

    /// <summary>
    /// New delete query with table name
    /// </summary>
    private static SqlKata.Query NewDeleteQuery(string table) =>
        NewQuery(table).AsDelete();

    /// <summary>
    /// New delete query with table name and object id condition
    /// </summary>
    public static SqlKata.Query NewDeleteQuery(string table, int objectId)
    {
        if (objectId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(objectId));
        }
        return NewDeleteQuery(table, ObjectColumn.Id, objectId);
    }

    /// <summary>
    /// New delete query with table name and field condition
    /// </summary>
    public static SqlKata.Query NewDeleteQuery(string table, string column, object value) =>
        NewDeleteQuery(table).Where(column, value);

    #endregion
}
