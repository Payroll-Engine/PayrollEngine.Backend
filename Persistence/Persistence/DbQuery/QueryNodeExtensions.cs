using System;
using System.Globalization;
using System.Linq;
using Microsoft.OData.UriParser;

namespace PayrollEngine.Persistence.DbQuery;

internal static class QueryNodeExtensions
{
    /// <summary>
    /// Gets the name of the column
    /// </summary>
    /// <param name="node">The query node</param>
    /// <param name="context">The query context</param>
    /// <returns>The column name</returns>
    internal static string GetColumnName(this QueryNode node, IQueryContext context)
    {
        if (node == null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        var column = string.Empty;
        if (node.Kind == QueryNodeKind.Convert)
        {
            node = ((ConvertNode)node).Source;
        }

        // column name
        if (node.Kind == QueryNodeKind.SingleValuePropertyAccess)
        {
            column = ((SingleValuePropertyAccessNode)node).Property.Name;
        }
        else if (node.Kind == QueryNodeKind.SingleValueOpenPropertyAccess)
        {
            column = ((SingleValueOpenPropertyAccessNode)node).Name;
        }
        else if (node.Kind == QueryNodeKind.SingleValueFunctionCall)
        {
            column = ((SingleValueFunctionCallNode)node).Name;
        }

        return context.ValidateColumn(column);
    }

    /// <summary>
    /// Gets the constant query value
    /// </summary>
    /// <param name="node">The query node</param>
    /// <returns>The query constant value</returns>
    internal static object GetConstantValue(this QueryNode node)
    {
        if (node == null)
        {
            throw new ArgumentNullException(nameof(node));
        }
        // constant
        if (node.Kind == QueryNodeKind.Constant)
        {
            return ((ConstantNode)node).Value;
        }
        return null;
    }

    /// <summary>
    /// Gets the constant query value
    /// </summary>
    /// <param name="node">The query node</param>
    /// <param name="columnName">The column name</param>
    /// <param name="columnType">The column type</param>
    /// <returns>The query constant value</returns>
    internal static object GetConstantValue(this QueryNode node, string columnName, Type columnType)
    {
        if (node == null)
        {
            throw new ArgumentNullException(nameof(node));
        }
        if (string.IsNullOrWhiteSpace(columnName))
        {
            throw new ArgumentException(nameof(columnName));
        }
        if (columnType == null)
        {
            throw new ArgumentNullException(nameof(columnType));
        }

        // convert
        if (node.Kind == QueryNodeKind.Convert)
        {
            return GetConstantValue(((ConvertNode)node).Source, columnName, columnType);
        }

        // constant
        if (node.Kind == QueryNodeKind.Constant)
        {
            var value = ((ConstantNode)node).Value;

            // string
            if ((columnType == typeof(DateTime) || columnType == typeof(DateTime?)) && value is string dateValue)
            {
                var trimmedValue = dateValue.Trim();
                if (ConvertToDateTimeUtc(trimmedValue, out var dateTime) && dateTime.HasValue)
                {
                    return dateTime.Value;
                }
                return trimmedValue;
            }

            // enum
            if (columnType.IsEnum && value is string enumString)
            {
                var trimmedValue = enumString.Trim();
                if (Enum.TryParse(columnType, trimmedValue, true, out var enumValue))
                {
                    return enumValue;
                }
                return trimmedValue;
            }

            return value;
        }

        // constant collection
        if (node.Kind == QueryNodeKind.CollectionConstant)
        {
            return ((CollectionConstantNode)node).Collection.Select(_ => GetConstantValue(node, columnName, columnType));
        }

        return null;
    }

    /// <summary>
    /// Converts to date time UTC
    /// </summary>
    /// <param name="dateTimeString">The date time string to convert</param>
    /// <param name="dateTime">The resulting date time</param>
    /// <returns>True for a valid input date time</returns>
    private static bool ConvertToDateTimeUtc(this string dateTimeString, out DateTime? dateTime)
    {
        DateTimeStyles DateTimeStyle = DateTimeStyles.AssumeUniversal |
                                       DateTimeStyles.AdjustToUniversal |
                                       DateTimeStyles.AllowWhiteSpaces;
        if (DateTime.TryParse(dateTimeString, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyle, out var dateTimeValue))
        {
            dateTime = dateTimeValue;
            return true;
        }
        dateTime = null;
        return false;
    }
}