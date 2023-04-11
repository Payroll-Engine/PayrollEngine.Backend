using System;

namespace PayrollEngine.Persistence.DbQuery;

/// <summary>
/// Database query context
/// </summary>
internal interface IQueryContext
{
    /// <summary>
    /// Get the column type
    /// </summary>
    /// <param name="name">The column name</param>
    /// <returns>The column type</returns>
    Type GetColumnType(string name);

    /// <summary>
    /// Validate a column
    /// </summary>
    /// <remarks>
    /// Throws an exception on invalid columns
    /// </remarks>
    /// <param name="name">The column name</param>
    /// <returns>The valid column name</returns>
    string ValidateColumn(string name);
}