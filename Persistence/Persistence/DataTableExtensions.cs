using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace PayrollEngine.Persistence;

/// <summary>
/// Extension methods for the <see cref="DataTable"/>
/// </summary>
internal static class DataTableExtensions
{
    /// <summary>
    /// Convert database parameters to data table
    /// </summary>
    /// <param name="parameterSet">Parameter set to convert</param>
    /// <param name="tableName">Target table name</param>
    internal static DataTable ToDataTable(this List<DbParameterCollection> parameterSet, string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException(nameof(tableName));
        }

        // table
        var dataTable = new DataTable(tableName);
        if (!parameterSet.Any())
        {
            return dataTable;
        }

        // columns
        var firstParameter = parameterSet.First();
        // include id column
        var columnCount = firstParameter.Count + 1;
        // column names cache
        var columnNames = new string[columnCount];

        // id column
        dataTable.Columns.Add(DbSchema.ObjectColumn.Id, typeof(int));
        columnNames[0] = DbSchema.ObjectColumn.Id;

        // value columns
        foreach (var name in firstParameter.ParameterNames)
        {
            var dbType = firstParameter.GetParameterType(name);
            var type = dbType == null ? typeof(string) : dbType.Value.ToSystemType();
            dataTable.Columns.Add(name, type);
            columnNames[dataTable.Columns.Count - 1] = name;
        }

        // process parallel partitions
        var partitioner = Partitioner.Create(parameterSet, EnumerablePartitionerOptions.NoBuffering);
        var preparedRows = partitioner.AsParallel().Select(parameterCollection =>
            {
                // row values
                var values = new object[columnCount];
                values[0] = 0; // ID placeholder

                for (var i = 1; i < columnCount; i++)
                {
                    // row value
                    values[i] = parameterCollection.Get<object>(columnNames[i]);
                }
                return values;
            }).ToList();

        // add rows
        // Suspend index/constraint checks
        dataTable.BeginLoadData();
        try
        {
            foreach (var rowData in preparedRows)
            {
                dataTable.Rows.Add(rowData);
            }
        }
        finally
        {
            // finalize loading
            dataTable.EndLoadData();
        }

        return dataTable;
    }
}