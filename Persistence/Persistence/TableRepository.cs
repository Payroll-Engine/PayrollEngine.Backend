using System;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

/// <summary>
/// Abstract base repository that maps to a specific database table.
/// Provides the table name used in all SQL operations.
/// </summary>
public abstract class TableRepository : RepositoryBase, IRepository
{
    protected string TableName { get; }

    /// <summary>
    /// The object type name
    /// </summary>
    public string TypeName => TableName;

    protected TableRepository(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException(nameof(tableName));
        }
        TableName = tableName;
    }

}