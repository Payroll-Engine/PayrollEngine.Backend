using System;
using System.Collections.Generic;
using System.Linq;

namespace PayrollEngine.Persistence.DbQuery;

internal class TypeQueryBuilder<T> : QueryBuilderBase, IQueryContext
{
    /// <summary>
    /// Available query columns
    /// </summary>
    private IDictionary<string, Type> TypeColumns { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryBuilderBase"/> class
    /// </summary>
    internal TypeQueryBuilder()
    {
        var properties = typeof(T).GetPropertyTypes();
        var typeColumns = new Dictionary<string, Type>();
        foreach (var property in properties)
        {
            if (property.Value.IsClass &&
                // exclude string and localizations
                !(property.Value == typeof(string) ||
                  property.Value == typeof(Dictionary<string, string>)))
            {
                continue;
            }
            typeColumns.Add(property.Key, property.Value);
        }
        TypeColumns = typeColumns;
    }

    /// <summary>
    /// Find a column by name
    /// </summary>
    /// <param name="name">The column name</param>
    /// <returns>Existing column name</returns>
    protected string FindTypeColumn(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(nameof(name));
        }
        return TypeColumns.Keys.FirstOrDefault(x => string.Equals(x, name.Trim(), StringComparison.InvariantCultureIgnoreCase));
    }

    #region IQueryContext

    /// <summary>
    /// The query context
    /// </summary>
    protected override IQueryContext QueryContext => this;

    Type IQueryContext.GetColumnType(string name) =>
        GetColumnType(name);

    /// <summary>
    /// Get the column type
    /// </summary>
    /// <param name="name">The column name</param>
    /// <returns>The column type</returns>
    protected virtual Type GetColumnType(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(nameof(name));
        }
        return TypeColumns.TryGetValue(name, out var column) ? column : null;
    }

    string IQueryContext.ValidateColumn(string name) =>
        ValidateColumn(name);

    /// <summary>
    /// Validate query column
    /// </summary>
    /// <param name="name">The column name</param>
    /// <returns>The valid column name</returns>
    protected virtual string ValidateColumn(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(nameof(name));
        }

        // case-insensitive column name
        var typeColumn = FindTypeColumn(name.Trim());
        if (typeColumn == null)
        {
            throw new QueryException($"Unknown query field {name}");
        }
        return typeColumn;
    }

    #endregion

}