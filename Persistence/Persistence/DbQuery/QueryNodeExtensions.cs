using System;
using System.Globalization;
using System.Linq;
using Microsoft.OData.UriParser;

namespace PayrollEngine.Persistence.DbQuery;

internal static class QueryNodeExtensions
{
    /// <param name="node">The query node</param>
    extension(QueryNode node)
    {
        /// <summary>
        /// Gets the name of the column
        /// </summary>
        /// <param name="context">The query context</param>
        /// <returns>The column name</returns>
        internal string GetColumnName(IQueryContext context)
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
        /// <returns>The query constant value</returns>
        internal object GetConstantValue()
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
        /// <param name="columnName">The column name</param>
        /// <param name="columnType">The column type</param>
        /// <returns>The query constant value</returns>
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Global
        internal object GetConstantValue(string columnName, Type columnType)
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
                return ((ConvertNode)node).Source.GetConstantValue(columnName, columnType);
            }

            // constant
            if (node.Kind == QueryNodeKind.Constant)
            {
                var value = ((ConstantNode)node).Value;

                // string
                if ((columnType == typeof(DateTime) || columnType == typeof(DateTime?)) && value is string dateValue)
                {
                    var trimmedValue = dateValue.Trim();
                    if (trimmedValue.ConvertToDateTimeUtc(out var dateTime) && dateTime.HasValue)
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
                return ((CollectionConstantNode)node).Collection.Select(_ => node.GetConstantValue(columnName, columnType));
            }

            return null;
        }
    }

    /// <summary>
    /// Converts to date time UTC
    /// </summary>
    /// <param name="dateTimeString">The date time string to convert</param>
    /// <param name="dateTime">The resulting date time</param>
    /// <returns>True for a valid input date time</returns>
    private static bool ConvertToDateTimeUtc(this string dateTimeString, out DateTime? dateTime)
    {
        var DateTimeStyle = DateTimeStyles.AssumeUniversal |
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