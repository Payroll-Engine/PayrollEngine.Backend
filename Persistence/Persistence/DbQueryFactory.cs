using System;
using System.Collections.Generic;
using System.Linq;
using PayrollEngine.Domain.Model;
using PayrollEngine.Persistence.DbQuery;
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

    private static SqlKata.Query BuildQuery(QueryBuilderBase queryBuilder, string tableName, Query query = null,
        DbQueryMode queryMode = DbQueryMode.Item)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException(nameof(tableName));
        }

        // build query including OData support
        SqlKata.Query dbQuery = queryBuilder.BuildQuery(tableName, query, queryMode);

        // object status
        if (query?.Status != null)
        {
            switch (query.Status)
            {
                case ObjectStatus.Active:
                    dbQuery.Where(DbSchema.ObjectColumn.Status, (int)ObjectStatus.Active);
                    break;
                case ObjectStatus.Inactive:
                    dbQuery.Where(DbSchema.ObjectColumn.Status, (int)ObjectStatus.Inactive);
                    break;
            }
        }

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
        NewQuery(tableName, DbSchema.ObjectColumn.Id, objectId);

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
        DbQueryMode queryMode = DbQueryMode.Item)
    {
        if (typeof(T).GetInterface(nameof(IDomainAttributeObject)) != null)
        {
            var dynamicQueryBuilder = new DynamicTypeQueryBuilder<T>(DbSchema.Prefixes.AttributePrefixes);
            var dbQuery = BuildQuery(dynamicQueryBuilder, tableName, query, queryMode);

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
                    dbQuery.SelectRaw(BuildAttributeQuery(column));
                    dynamicColumn = true;
                }
                else if (column.IsDateAttributeField())
                {
                    dbQuery.SelectRaw(BuildAttributeQuery(column, dbContext.DateTimeType));
                    dynamicColumn = true;
                }
                else if (column.IsNumericAttributeField())
                {
                    dbQuery.SelectRaw(BuildAttributeQuery(column, dbContext.DecimalType));
                    dynamicColumn = true;
                }
            }

            // ensure columns
            if (dynamicColumn && !dbQuery.Clauses.Any(x => x is Column))
            {
                dbQuery.Select("*");
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
            if (queryMode == DbQueryMode.ItemCount)
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
        return BuildQuery(queryBuilder, tableName, query, queryMode);
    }

    private static string BuildAttributeQuery(string column, string valueAlias = null)
    {
        // ReSharper disable StringLiteralTypo
        var attribute = column.RemoveAttributePrefix();
        if (string.IsNullOrWhiteSpace(valueAlias))
        {
            return "(SELECT value FROM OPENJSON(Attributes) " + "" +
                   $"WHERE [key] = '{attribute}') AS {column}";
        }
        return $"(SELECT CAST(value AS {valueAlias}) FROM OPENJSON(Attributes) " + "" +
                           $"WHERE [key] = '{attribute}') AS {column}";
        // ReSharper restore StringLiteralTypo
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
        Query query = null, DbQueryMode queryMode = DbQueryMode.Item)
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
        DbQueryMode queryMode = DbQueryMode.Item)
    {
        var queryBuilder = new DynamicTypeQueryBuilder<T>(DbSchema.Prefixes.AttributePrefixes);
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
        Query query = null, DbQueryMode queryMode = DbQueryMode.Item)
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
        return NewCountQuery(table, DbSchema.ObjectColumn.Id, objectId);
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
    public static SqlKata.Query NewDeleteQuery(string table) =>
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
        return NewDeleteQuery(table, DbSchema.ObjectColumn.Id, objectId);
    }

    /// <summary>
    /// New delete query with table name and field condition
    /// </summary>
    public static SqlKata.Query NewDeleteQuery(string table, string column, object value) =>
        NewDeleteQuery(table).Where(column, value);

    #endregion
}