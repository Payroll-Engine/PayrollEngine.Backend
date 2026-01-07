using System;
using System.Linq;
using SqlKata;
// ReSharper disable UnusedMethodReturnValue.Global

namespace PayrollEngine.Persistence;

public static class SqlQueryExtensions
{
    extension(SqlKata.Query query)
    {
        /// <summary>
        /// Change the table name from the FROM clause
        /// </summary>
        public SqlKata.Query ChangeTableName(string table)
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
        public SqlKata.Query SelectAll(string table)
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
        public SqlKata.Query WhereNullOrValue(string column, object value)
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
        public SqlKata.Query WhereNullOrValue(string column, string op, object value)
        {
            query.Where(q =>
                q.WhereNull(column)
                    .OrWhere(column, op, value));
            return query;
        }

        /// <summary>
        /// Add a where condition to a related table
        /// </summary>
        public SqlKata.Query RelatedWhere(string relatedTable, string relatedColumn, object value)
        {
            query?.Where(relatedTable.ToTableColumn(relatedColumn), value);
            return query;
        }

        /// <summary>
        /// Add a nullable where condition to a related table
        /// </summary>
        public SqlKata.Query RelatedWhereNullOrValue(string relatedTable, string relatedColumn, object value)
        {
            query?.WhereNullOrValue(relatedTable.ToTableColumn(relatedColumn), value);
            return query;
        }

        /// <summary>
        /// Execute left join, using the object id as relation source
        /// </summary>
        public SqlKata.Query LeftObjectJoin(string sourceTable, string targetTable, string targetColumn) => 
            query.LeftObjectJoin(sourceTable, DbSchema.ObjectColumn.Id, targetTable, targetColumn);

        /// <summary>
        /// Execute left join
        /// </summary>
        private SqlKata.Query LeftObjectJoin(string sourceTable, string sourceColumn, string targetTable, string targetColumn)
        {
            query?.LeftJoin(targetTable, sourceTable.ToTableColumn(sourceColumn), targetTable.ToTableColumn(targetColumn));
            return query;
        }

        /// <summary>
        /// Adds the division filter
        /// </summary>
        public void AddDivisionFilter(DivisionScope divisionScope, int? divisionId)
        {
            if ((divisionScope == DivisionScope.Local || divisionScope == DivisionScope.GlobalAndLocal)
                && !divisionId.HasValue)
            {
                throw new PayrollException("Missing division to query local case values.");
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
        tableName.ToTableColumn("*");
}