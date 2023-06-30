﻿using System;
using System.Linq;
using SqlKata;
// ReSharper disable UnusedMethodReturnValue.Global

namespace PayrollEngine.Persistence;

public static class SqlQueryExtensions
{
    /// <summary>
    /// Change the table name from the FROM clause
    /// </summary>
    public static SqlKata.Query ChangeTableName(this SqlKata.Query query, string table)
    {
        if (string.IsNullOrWhiteSpace(table))
        {
            throw new ArgumentException(nameof(table));
        }

        if (query?.Clauses.FirstOrDefault(x => x is FromClause) is FromClause fromClause)
        {
            fromClause.Table = table;
        }
        return query;
    }

    /// <summary>
    /// Selects all table columns
    /// </summary>
    public static SqlKata.Query SelectAll(this SqlKata.Query query, string table)
    {
        if (string.IsNullOrWhiteSpace(table))
        {
            throw new ArgumentException(nameof(table));
        }
        query?.Select(ToAllTableColumns(table));
        return query;
    }

    /// <summary>
    /// Add a null conditional where
    /// T-SQL: ([column] IS NULL OR [column] = value)
    /// </summary>
    public static SqlKata.Query WhereNullOrValue(this SqlKata.Query query, string column, object value)
    {
        query.Where(q =>
            q.WhereNull(column)
                .OrWhere(column, value));
        return query;
    }

    /// <summary>
    /// Add a null conditional where
    /// T-SQL: ([column] IS NULL OR [column] 'op' value)
    /// </summary>
    public static SqlKata.Query WhereNullOrValue(this SqlKata.Query query, string column, string op, object value)
    {
        query.Where(q =>
            q.WhereNull(column)
                .OrWhere(column, op, value));
        return query;
    }

    /// <summary>
    /// Add a where condition to a related table
    /// </summary>
    public static SqlKata.Query RelatedWhere(this SqlKata.Query query, string relatedTable, string relatedColumn, object value)
    {
        query?.Where(ToTableColumn(relatedTable, relatedColumn), value);
        return query;
    }

    /// <summary>
    /// Add a nullable where condition to a related table
    /// </summary>
    public static SqlKata.Query RelatedWhereNullOrValue(this SqlKata.Query query, string relatedTable, string relatedColumn, object value)
    {
        query?.WhereNullOrValue(ToTableColumn(relatedTable, relatedColumn), value);
        return query;
    }

    /// <summary>
    /// Execute left join, using the object id as relation source
    /// </summary>
    public static SqlKata.Query LeftObjectJoin(this SqlKata.Query query, string sourceTable, string targetTable, string targetColumn) =>
        LeftObjectJoin(query, sourceTable, DbSchema.ObjectColumn.Id, targetTable, targetColumn);

    /// <summary>
    /// Execute left join
    /// </summary>
    private static SqlKata.Query LeftObjectJoin(this SqlKata.Query query, string sourceTable, string sourceColumn, string targetTable, string targetColumn)
    {
        query?.LeftJoin(targetTable,
            ToTableColumn(sourceTable, sourceColumn),
            ToTableColumn(targetTable, targetColumn));
        return query;
    }

    /// <summary>
    /// Adds the division filter
    /// </summary>
    public static void AddDivisionFilter(this SqlKata.Query query, DivisionScope divisionScope, int? divisionId)
    {
        if ((divisionScope == DivisionScope.Local || divisionScope == DivisionScope.GlobalAndLocal)
            && !divisionId.HasValue)
        {
            throw new PayrollException("Missing division to query local case values");
        }

        switch (divisionScope)
        {
            case DivisionScope.Local:
                // ReSharper disable once PossibleInvalidOperationException
                query.Where(DbSchema.CaseValueColumn.DivisionId, divisionId.Value);
                break;
            case DivisionScope.Global:
                query.WhereNull(DbSchema.CaseValueColumn.DivisionId);
                break;
            case DivisionScope.GlobalAndLocal:
                // db condition: ([DivisionId] IS NULL OR [DivisionId] = divisionId)
                // ReSharper disable once PossibleInvalidOperationException
                query.WhereNullOrValue(DbSchema.CaseValueColumn.DivisionId, divisionId.Value);
                break;
            default:
                if (divisionId.HasValue)
                {
                    query.Where(DbSchema.CaseValueColumn.DivisionId, divisionId.Value);
                }
                break;
        }
    }

    public static string ToTableColumn(this string tableName, string columnName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException(nameof(tableName));
        }
        if (string.IsNullOrWhiteSpace(columnName))
        {
            throw new ArgumentException(nameof(columnName));
        }
        return $"{tableName}.{columnName}";
    }

    private static string ToAllTableColumns(string tableName) =>
        ToTableColumn(tableName, "*");
}