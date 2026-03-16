using System;
using System.Linq;
using PayrollEngine.Domain.Model;
using PayrollEngine.Persistence.DbSchema;
using SqlKata;

namespace PayrollEngine.Persistence;

public static class SqlKataQueryExtensions
{
    /// <summary>
    /// Apply a case change query to a sql query
    /// </summary>
    /// <param name="query">The case change query</param>
    /// <param name="sqlQuery">The sql query</param>
    public static void ApplyTo(this CaseChangeQuery query, SqlKata.Query sqlQuery)
    {
        if (query == null)
        {
            throw new ArgumentNullException(nameof(query));
        }
        if (sqlQuery == null)
        {
            throw new ArgumentNullException(nameof(sqlQuery));
        }

        // division
        if (query.DivisionId.HasValue)
        {
            if (query.ExcludeGlobal)
            {
                sqlQuery.Where(CaseChangeColumn.DivisionId, query.DivisionId.Value);
            }
            else
            {
                // include global case changes
                sqlQuery.WhereNullOrValue(CaseChangeColumn.DivisionId, query.DivisionId.Value);
            }
        }
    }

    /// <summary>
    /// Apply a case value query to a sql query
    /// </summary>
    /// <param name="query">The case change query</param>
    /// <param name="sqlQuery">The sql query</param>
    public static void ApplyTo(this Domain.Model.CaseValueQuery query, SqlKata.Query sqlQuery)
    {
        if (query == null)
        {
            throw new ArgumentNullException(nameof(query));
        }
        if (sqlQuery == null)
        {
            throw new ArgumentNullException(nameof(sqlQuery));
        }

        sqlQuery.AddDivisionFilter(query.DivisionScope, query.DivisionId);
        if (!string.IsNullOrWhiteSpace(query.CaseName))
        {
            sqlQuery.Where(CaseValueColumn.CaseName, query.CaseName);
        }
        if (!string.IsNullOrWhiteSpace(query.CaseFieldName))
        {
            sqlQuery.Where(CaseValueColumn.CaseFieldName, query.CaseFieldName);
        }
    }

    /// <summary>
    /// Apply a case division query to a sql query
    /// </summary>
    /// <param name="query">The case change query</param>
    /// <param name="sqlQuery">The sql query</param>
    /// <param name="tableName">The table name</param>
    public static void ApplyTo(this DivisionQuery query, SqlKata.Query sqlQuery, string tableName)
    {
        if (query == null)
        {
            throw new ArgumentNullException(nameof(query));
        }
        if (sqlQuery == null)
        {
            throw new ArgumentNullException(nameof(sqlQuery));
        }
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException(nameof(tableName));
        }

        // division query
        if (query.DivisionId.HasValue)
        {
            // query
            if (sqlQuery.Clauses.FirstOrDefault(x => x is BasicCondition condition &&
                                                     string.Equals(ObjectColumn.Status, condition.Column)) is BasicCondition statusCondition)
            {
                sqlQuery.Clauses.Remove(statusCondition);
                sqlQuery.Where(tableName.ToTableColumn(ObjectColumn.Status), query.Status);
            }
            sqlQuery.SelectAll(Tables.Employee);

            // relation between employee and employee-division
            sqlQuery.LeftObjectJoin(Tables.Employee,
                Tables.EmployeeDivision, EmployeeDivisionColumn.EmployeeId);

            // division filter
            sqlQuery.RelatedWhere(Tables.EmployeeDivision,
                EmployeeDivisionColumn.DivisionId, query.DivisionId.Value);
        }
    }

    /// <summary>
    /// Apply a report template query to a sql query
    /// </summary>
    /// <param name="query">The report template query</param>
    /// <param name="sqlQuery">The sql query</param>
    public static void ApplyTo(this ReportTemplateQuery query, SqlKata.Query sqlQuery)
    {
        if (query == null)
        {
            throw new ArgumentNullException(nameof(query));
        }
        if (sqlQuery == null)
        {
            throw new ArgumentNullException(nameof(sqlQuery));
        }

        // language
        if (!string.IsNullOrWhiteSpace(query.Culture))
        {
            sqlQuery.Where(ReportTemplateColumn.Culture, query.Culture);
        }
    }
}