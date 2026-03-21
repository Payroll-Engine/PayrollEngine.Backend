using System;
using System.Collections.Generic;
using System.Linq;

namespace PayrollEngine.Persistence.DbQuery;

internal class TypeQueryBuilder<T> : QueryBuilderBase, IQueryContext
{
    /// <summary>
    /// Available scalar query columns (value types, string, enum)
    /// </summary>
    private IDictionary<string, Type> TypeColumns { get; }

    /// <summary>
    /// JSON collection columns (List&lt;T&gt; of primitives/string) — eligible for any() lambda queries
    /// </summary>
    private IDictionary<string, Type> CollectionColumns { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeQueryBuilder{T}"/> class
    /// </summary>
    internal TypeQueryBuilder()
    {
        var properties = typeof(T).GetPropertyTypes();
        var typeColumns = new Dictionary<string, Type>();
        var collectionColumns = new Dictionary<string, Type>();

        foreach (var property in properties)
        {
            if (property.Value.IsClass)
            {
                // scalar string and localization dictionary → regular columns
                if (property.Value == typeof(string) ||
                    property.Value == typeof(Dictionary<string, string>))
                {
                    typeColumns.Add(property.Key, property.Value);
                }
                // List<primitive|string|decimal|DateTime|Guid> → JSON collection column
                else if (IsJsonCollectionType(property.Value))
                {
                    collectionColumns.Add(property.Key, property.Value);
                }
                // other complex classes → skip
            }
            else
            {
                typeColumns.Add(property.Key, property.Value);
            }
        }

        TypeColumns = typeColumns;
        CollectionColumns = collectionColumns;
    }

    /// <summary>
    /// Returns true when the type is a List&lt;T&gt; where T is a JSON-serializable primitive
    /// </summary>
    private static bool IsJsonCollectionType(Type type)
    {
        if (!type.IsGenericType)
        {
            return false;
        }
        if (type.GetGenericTypeDefinition() != typeof(List<>))
        {
            return false;
        }
        var elementType = type.GetGenericArguments()[0];
        return elementType.IsPrimitive ||
               elementType == typeof(string) ||
               elementType == typeof(decimal) ||
               elementType == typeof(DateTime) ||
               elementType == typeof(Guid);
    }

    /// <summary>
    /// Find a scalar column by name (case-insensitive)
    /// </summary>
    /// <param name="name">The column name</param>
    /// <returns>Existing column name, or null</returns>
    protected string FindTypeColumn(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(nameof(name));
        }
        return TypeColumns.Keys.FirstOrDefault(x =>
            string.Equals(x, name.Trim(), StringComparison.InvariantCultureIgnoreCase));
    }

    /// <summary>
    /// Find a JSON collection column by name (case-insensitive)
    /// </summary>
    /// <param name="name">The column name</param>
    /// <returns>Existing collection column name, or null</returns>
    protected string FindCollectionColumn(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(nameof(name));
        }
        return CollectionColumns.Keys.FirstOrDefault(x =>
            string.Equals(x, name.Trim(), StringComparison.InvariantCultureIgnoreCase));
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
    /// Validate query column — accepts both scalar and collection columns
    /// </summary>
    /// <param name="name">The column name</param>
    /// <returns>The valid column name</returns>
    protected virtual string ValidateColumn(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(nameof(name));
        }

        var trimmed = name.Trim();

        // scalar column
        var typeColumn = FindTypeColumn(trimmed);
        if (typeColumn != null)
        {
            return typeColumn;
        }

        // JSON collection column (any() source)
        var collectionColumn = FindCollectionColumn(trimmed);
        if (collectionColumn != null)
        {
            return collectionColumn;
        }

        throw new QueryException($"Unknown query field {name}");
    }

    bool IQueryContext.IsCollectionColumn(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }
        return FindCollectionColumn(name) != null;
    }

    #endregion
}
