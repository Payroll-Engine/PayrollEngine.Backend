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

    /// <summary>
    /// Returns true if the column is a JSON collection (e.g. List&lt;string&gt;)
    /// and can be used as the source of an OData any() lambda expression
    /// </summary>
    /// <param name="name">The column name</param>
    /// <returns>True when the column holds a JSON-serialized collection</returns>
    bool IsCollectionColumn(string name);
}
