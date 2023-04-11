using System;
using System.Linq;
using Microsoft.OData.UriParser.Aggregation;

namespace PayrollEngine.Persistence.DbQuery;

/// <summary>
/// Build apply clause
/// </summary>
internal sealed class ApplyClauseBuilder
{
    /// <summary>
    /// The query context
    /// </summary>
    private IQueryContext QueryContext { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplyClauseBuilder"/> class
    /// </summary>
    /// <param name="queryContext">The query context</param>
    internal ApplyClauseBuilder(IQueryContext queryContext)
    {
        QueryContext = queryContext ?? throw new ArgumentNullException(nameof(queryContext));
    }

    /// <summary>
    /// Build apply clause
    /// </summary>
    /// <param name="query">The query</param>
    /// <param name="applyClause">The app clause</param>
    /// <returns>The updated query</returns>
    internal SqlKata.Query BuildApplyClause(SqlKata.Query query, ApplyClause applyClause)
    {
        if (query is null)
        {
            throw new ArgumentNullException(nameof(query));
        }
        if (applyClause is null)
        {
            throw new ArgumentNullException(nameof(applyClause));
        }

        var i = 0;
        foreach (var node in applyClause.Transformations)
        {
            // Supported nodes, Aggregate is always the end node
            // 1. Aggregate
            // 2. GroupBy
            // 3. GroupBy / Aggregate
            // 4. Filter / GroupBy
            // 5. Filter / Aggregate
            // 6. Filter / GroupBy / Filter
            // 7. Filter / GroupBy / Aggregate
            // 8. Filter / GroupBy / GroupBy .../ GroupBy / Filter
            if (i > 0 && applyClause.Transformations.ElementAt(i - 1).Kind != TransformationNodeKind.Filter)
            {
                // Use sub query if prev transformation is not filter
                query = new SqlKata.Query().From(query);
            }

            switch (node.Kind)
            {
                case TransformationNodeKind.Aggregate:
                    query = Visit(query, node as AggregateTransformationNode);
                    // Aggregate is end node so return here
                    return query;
                case TransformationNodeKind.GroupBy:
                    query = Visit(query, node as GroupByTransformationNode);
                    break;
                case TransformationNodeKind.Filter:
                    query = Visit(query, node as FilterTransformationNode);
                    break;
                case TransformationNodeKind.Compute:
                case TransformationNodeKind.Expand:
                    goto default;
                default:
                    throw new QueryException($"Transformation node {node.Kind:g} not supported");
            }
            i++;
        }
        return query;
    }

    private SqlKata.Query Visit(SqlKata.Query query, AggregateTransformationNode nodeIn)
    {
        foreach (var expr in nodeIn.AggregateExpressions.OfType<AggregateExpression>())
        {
            if (expr.AggregateKind == AggregateExpressionKind.PropertyAggregate)
            {
                switch (expr.Method)
                {
                    case AggregationMethod.Sum:
                    case AggregationMethod.Min:
                    case AggregationMethod.Max:
                        query = query.SelectRaw($"{expr.Method:g}({expr.Expression.GetColumnName(QueryContext)}) AS {expr.Alias}");
                        break;
                    case AggregationMethod.Average:
                        query = query.SelectRaw($"AVG({expr.Expression.GetColumnName(QueryContext)}) AS {expr.Alias}");
                        break;
                    case AggregationMethod.CountDistinct:
                        query = query.SelectRaw($"COUNT(DISTINCT {expr.Expression.GetColumnName(QueryContext)}) AS {expr.Alias}");
                        break;
                    case AggregationMethod.VirtualPropertyCount:
                        query = query.SelectRaw($"COUNT(1) AS {expr.Alias}");
                        break;
                    case AggregationMethod.Custom:
                        goto default;
                    default:
                        throw new QueryException($"Aggregate method {expr.Method:g} not supported");
                }
            }
        }

        return query;
    }

    private SqlKata.Query Visit(SqlKata.Query query, GroupByTransformationNode nodeIn)
    {
        foreach (var groupByProperty in nodeIn.GroupingProperties)
        {
            var columnName = groupByProperty.Expression.GetColumnName(QueryContext);
            query = query.Select(columnName).GroupBy(columnName);
        }

        if (nodeIn.ChildTransformations?.Kind == TransformationNodeKind.Aggregate)
        {
            query = Visit(query, nodeIn.ChildTransformations as AggregateTransformationNode);
        }

        return query;
    }

    private SqlKata.Query Visit(SqlKata.Query query, FilterTransformationNode nodeIn)
    {
        var filterClause = nodeIn.FilterClause.Expression;
        var filterClauseBuilder = new FilterClauseBuilder(query, QueryContext);
        return filterClause.Accept(filterClauseBuilder);
    }
}