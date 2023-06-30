using System;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

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