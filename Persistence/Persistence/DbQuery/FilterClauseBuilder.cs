using System;
using System.Globalization;
using System.Linq;
using Microsoft.OData.UriParser;

namespace PayrollEngine.Persistence.DbQuery;

/// <summary>
/// Build filter clause
/// </summary>
internal sealed class FilterClauseBuilder : QueryNodeVisitor<SqlKata.Query>
{
    private SqlKata.Query query;

    /// <summary>
    /// The query context
    /// </summary>
    private IQueryContext QueryContext { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterClauseBuilder"/> class
    /// </summary>
    /// <param name="query">The query</param>
    /// <param name="queryContext">The query context</param>
    internal FilterClauseBuilder(SqlKata.Query query, IQueryContext queryContext)
    {
        this.query = query ?? throw new ArgumentNullException(nameof(query));
        QueryContext = queryContext ?? throw new ArgumentNullException(nameof(queryContext));
    }

    /// <inheritdoc/>
    public override SqlKata.Query Visit(BinaryOperatorNode nodeIn)
    {
        // left node
        var left = nodeIn.Left;
        if (left.Kind == QueryNodeKind.Convert)
        {
            left = ((ConvertNode)left).Source;
        }

        // right node
        var right = nodeIn.Right;
        if (right.Kind == QueryNodeKind.Convert)
        {
            right = ((ConvertNode)right).Source;
        }

        // operator
        switch (nodeIn.OperatorKind)
        {
            case BinaryOperatorKind.Or:
            case BinaryOperatorKind.And:
                query = query.Where(q =>
                {
                    var lb = new FilterClauseBuilder(q, QueryContext);
                    var lq = left.Accept(lb);
                    if (nodeIn.OperatorKind == BinaryOperatorKind.Or)
                    {
                        lq = lq.Or();
                    }
                    var rb = new FilterClauseBuilder(lq, QueryContext);
                    return right.Accept(rb);
                });
                break;

            case BinaryOperatorKind.Equal:
            case BinaryOperatorKind.NotEqual:
            case BinaryOperatorKind.GreaterThan:
            case BinaryOperatorKind.GreaterThanOrEqual:
            case BinaryOperatorKind.LessThan:
            case BinaryOperatorKind.LessThanOrEqual:
                var op = GetOperatorString(nodeIn.OperatorKind);
                if (left.Kind == QueryNodeKind.UnaryOperator)
                {
                    query = query.Where(q =>
                    {
                        var lb = new FilterClauseBuilder(q, QueryContext);
                        // ReSharper disable once AccessToModifiedClosure
                        return left.Accept(lb);
                    });
                    left = ((UnaryOperatorNode)left).Operand;
                }
                if (right.Kind == QueryNodeKind.Constant)
                {
                    if (left.Kind == QueryNodeKind.SingleValueFunctionCall)
                    {
                        var value = right.GetConstantValue();
                        var functionNode = left as SingleValueFunctionCallNode;
                        query = ApplyFunction(query, functionNode, op, value);
                    }
                    else
                    {
                        var column = left.GetColumnName(QueryContext);
                        var columnType = QueryContext.GetColumnType(column);
                        var value = columnType != null ?
                            right.GetConstantValue(column, columnType) :
                            right.GetConstantValue();
                        query = query.Where(column, op, value);
                    }
                }
                break;

            default:
                return query;
        }

        return query;
    }

    /// <inheritdoc/>
    public override SqlKata.Query Visit(SingleValueFunctionCallNode nodeIn)
    {
        if (nodeIn is null)
        {
            throw new ArgumentNullException(nameof(nodeIn));
        }

        var nodes = nodeIn.Parameters.ToArray();
        var column = nodes[0].GetColumnName(QueryContext);
        var columnType = QueryContext.GetColumnType(column);

        var value = columnType != null ?
            (string)nodes[1].GetConstantValue(column, columnType) :
            (string)nodes[1].GetConstantValue();
        var functionName = nodeIn.Name.ToLowerInvariant();
        if (string.Equals(functionName, QuerySpecification.ContainsFunction))
        {
            return query.WhereContains(column, value, true);
        }
        if (string.Equals(functionName, QuerySpecification.NotContainsFunction))
        {
            return query.WhereNotContains(column, value, true);
        }
        if (string.Equals(functionName, QuerySpecification.EndsWithFunction))
        {
            return query.WhereEnds(column, value, true);
        }
        if (string.Equals(functionName, QuerySpecification.StartsWithFunction))
        {
            return query.WhereStarts(column, value, true);
        }
        return query;
    }

    /// <inheritdoc/>
    public override SqlKata.Query Visit(UnaryOperatorNode nodeIn)
    {
        switch (nodeIn.OperatorKind)
        {
            case UnaryOperatorKind.Not:
                query = query.Not();
                if (nodeIn.Operand.Kind == QueryNodeKind.SingleValueFunctionCall || nodeIn.Operand.Kind == QueryNodeKind.BinaryOperator)
                {
                    return nodeIn.Operand.Accept(this);
                }
                return query;
            default:
                return query;
        }
    }

    private SqlKata.Query ApplyFunction(SqlKata.Query functionQuery, SingleValueFunctionCallNode leftNode, string operand, object rightValue)
    {
        var columnName = leftNode.Parameters.FirstOrDefault()?.GetColumnName(QueryContext);
        if (!string.IsNullOrWhiteSpace(columnName))
        {
            switch (leftNode.Name.ToUpperInvariant())
            {
                case "YEAR":
                case "MONTH":
                case "DAY":
                case "HOUR":
                case "MINUTE":
                    functionQuery = functionQuery.WhereDatePart(leftNode.Name, columnName, operand, rightValue);
                    break;
                case "DATE":
                    functionQuery = functionQuery.WhereDate(columnName, operand, rightValue is DateTime date ?
                        date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture.DateTimeFormat) :
                        rightValue);
                    break;
                case "TIME":
                    functionQuery = functionQuery.WhereTime(columnName, operand, rightValue is DateTime time ?
                        time.ToString("HH:mm", CultureInfo.InvariantCulture.DateTimeFormat) :
                        rightValue);
                    break;
            }
        }
        return functionQuery;
    }

    private static string GetOperatorString(BinaryOperatorKind operatorKind)
    {
        return operatorKind switch
        {
            BinaryOperatorKind.Equal => "=",
            BinaryOperatorKind.NotEqual => "<>",
            BinaryOperatorKind.GreaterThan => ">",
            BinaryOperatorKind.GreaterThanOrEqual => ">=",
            BinaryOperatorKind.LessThan => "<",
            BinaryOperatorKind.LessThanOrEqual => "<=",
            BinaryOperatorKind.Or => "or",
            BinaryOperatorKind.And => "and",
            BinaryOperatorKind.Add => "+",
            BinaryOperatorKind.Subtract => "-",
            BinaryOperatorKind.Multiply => "*",
            BinaryOperatorKind.Divide => "/",
            BinaryOperatorKind.Modulo => "%",
            _ => string.Empty
        };
    }
}