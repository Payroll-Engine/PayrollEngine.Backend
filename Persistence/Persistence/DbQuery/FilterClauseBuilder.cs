using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.OData.UriParser;
using PayrollEngine.Domain.Model;

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
    /// The database context — used to build db-specific FROM fragments for any() EXISTS sub-queries.
    /// Null is accepted: <see cref="BuildCollectionFromRaw"/> falls back to SQL Server syntax.
    /// </summary>
    private IDbContext DbContext { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterClauseBuilder"/> class
    /// </summary>
    /// <param name="query">The query</param>
    /// <param name="queryContext">The query context</param>
    /// <param name="dbContext">The database context (optional — defaults to SQL Server syntax)</param>
    internal FilterClauseBuilder(SqlKata.Query query, IQueryContext queryContext, IDbContext dbContext = null)
    {
        this.query = query ?? throw new ArgumentNullException(nameof(query));
        QueryContext = queryContext ?? throw new ArgumentNullException(nameof(queryContext));
        DbContext = dbContext;
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
                    var lb = new FilterClauseBuilder(q, QueryContext, DbContext);
                    var lq = left.Accept(lb);
                    if (nodeIn.OperatorKind == BinaryOperatorKind.Or)
                    {
                        lq = lq.Or();
                    }
                    var rb = new FilterClauseBuilder(lq, QueryContext, DbContext);
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
                        var lb = new FilterClauseBuilder(q, QueryContext, DbContext);
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

    /// <inheritdoc/>
    /// <summary>
    /// Translates an OData any() lambda into a correlated EXISTS sub-query.
    /// The FROM fragment is generated by <see cref="IDbContext.BuildCollectionFromRaw"/> so the
    /// correct syntax is used for each database backend.
    ///
    /// SQL Server — scalar:    EXISTS (SELECT 1 FROM OPENJSON([Divisions]) WHERE [value] = @p0)
    /// SQL Server — key/value: EXISTS (SELECT 1 FROM OPENJSON([Attributes]) WITH ([Key] …) WHERE …)
    ///
    /// MySQL — scalar:         EXISTS (SELECT 1 FROM JSON_TABLE(`Divisions`, '$[*]' COLUMNS (value VARCHAR(255) PATH '$')) jt WHERE jt.value = @p0)
    /// MySQL — key/value:      EXISTS (SELECT 1 FROM JSON_TABLE(`Attributes`, '$[*]' COLUMNS (`Key` VARCHAR(255) PATH '$.key', …)) jt WHERE …)
    /// </summary>
    public override SqlKata.Query Visit(AnyNode nodeIn)
    {
        if (nodeIn is null)
        {
            throw new ArgumentNullException(nameof(nodeIn));
        }

        // Resolve the collection column name from the AnyNode source
        string collectionName;
        switch (nodeIn.Source)
        {
            case CollectionOpenPropertyAccessNode open:
                collectionName = open.Name;
                break;
            case CollectionPropertyAccessNode typed:
                collectionName = typed.Property.Name;
                break;
            default:
                throw new QueryException($"Unsupported any() source node kind: {nodeIn.Source?.Kind}");
        }

        var columnName = QueryContext.ValidateColumn(collectionName);

        if (!QueryContext.IsCollectionColumn(columnName))
        {
            throw new QueryException(
                $"Field '{columnName}' is not a collection field and cannot be used with any()");
        }

        var body = nodeIn.Body as BinaryOperatorNode
            ?? throw new QueryException("The any() lambda body must be a comparison expression (BinaryOperatorNode)");

        var conditions = CollectAnyConditions(body);

        // Key-value flat object (Dictionary<string, object>): route to db-specific flat-object handler.
        // SQL Server: EXISTS (SELECT 1 FROM OPENJSON([col]) WHERE [Key]=@p0 [AND [Value]=@p1])
        // MySQL:      direct WHERE via JSON_CONTAINS_PATH / JSON_UNQUOTE(JSON_EXTRACT(...))
        if (QueryContext.IsKeyValueColumn(columnName))
        {
            var rawWhere = DbContext?.BuildFlatObjectAnyWhere(columnName, conditions);
            if (rawWhere.HasValue)
            {
                // MySQL: direct WHERE expression — no EXISTS subquery
                query = query.WhereRaw(rawWhere.Value.RawSql, rawWhere.Value.Bindings);
            }
            else
            {
                // SQL Server: EXISTS (SELECT 1 FROM OPENJSON([col]) WHERE [key]=@p0 ...)
                // OPENJSON without WITH exposes built-in [key] / [value] / [type] columns — always lowercase.
                // PE uses a case-sensitive collation (SQL_Latin1_General_CP1_CS_AS), so column names
                var fromRaw = $"OPENJSON([{columnName}])";
                // must be lowercased to match the OPENJSON output ([key], not [Key]).
                query = query.WhereExists(q =>
                {
                    q = q.FromRaw(fromRaw);
                    foreach (var (col, op, val) in conditions)
                    {
                        q = q.Where(col.ToLowerInvariant(), op, val);
                    }
                    return q;
                });
            }
            return query;
        }

        // Scalar array (List<T>): single condition whose column is "value" → no property schema needed
        var isScalar = conditions.Count == 1 &&
                       string.Equals(conditions[0].Column, "value", StringComparison.OrdinalIgnoreCase);

        var propertyNames = isScalar
            ? []
            : conditions.Select(c => c.Column).ToArray();

        var fromRaw2 = BuildCollectionFromRaw(columnName, isScalar, propertyNames);

        query = query.WhereExists(q =>
        {
            q = q.FromRaw(fromRaw2);
            foreach (var (col, op, val) in conditions)
            {
                q = q.Where(col, op, val);
            }
            return q;
        });

        return query;
    }

    /// <inheritdoc/>
    /// <summary>
    /// Translates an OData in expression into a SQL WHERE col IN (...) clause.
    ///
    ///   payrollId in (1, 2, 3)   →  WHERE [payrollId] IN (@p0, @p1, @p2)
    ///   Name in ('Alice', 'Bob') →  WHERE [Name] IN (@p0, @p1)
    /// </summary>
    public override SqlKata.Query Visit(InNode nodeIn)
    {
        if (nodeIn is null)
        {
            throw new ArgumentNullException(nameof(nodeIn));
        }

        var column = nodeIn.Left.GetColumnName(QueryContext);
        var columnType = QueryContext.GetColumnType(column);

        if (nodeIn.Right is CollectionConstantNode collection)
        {
            var values = collection.Collection
                .Select(item => columnType != null
                    ? item.GetConstantValue(column, columnType)
                    : item.GetConstantValue())
                .ToList();

            query = query.WhereIn(column, values);
        }

        return query;
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns the db-specific FROM fragment for an any() EXISTS sub-query.
    /// Delegates to <see cref="IDbContext.BuildCollectionFromRaw"/> when a DbContext is available,
    /// otherwise falls back to the SQL Server OPENJSON syntax (used in unit tests where DbContext is null).
    /// </summary>
    private string BuildCollectionFromRaw(string columnName, bool isScalar, IReadOnlyList<string> propertyNames)
    {
        if (DbContext != null)
        {
            return DbContext.BuildCollectionFromRaw(columnName, isScalar, propertyNames);
        }

        // Fallback: SQL Server syntax — used when DbContext is null (unit tests, NewTypeQuery path)
        if (isScalar)
        {
            return $"OPENJSON([{columnName}])";
        }
        var withParts = propertyNames.Select(p => $"[{p}] NVARCHAR(MAX) '$.{p.ToLowerInvariant()}'");
        return $"OPENJSON([{columnName}]) WITH ({string.Join(", ", withParts)})";
    }

    /// <summary>
    /// Recursively extracts (column, operator, value) triples from the lambda body.
    /// Handles:
    ///   - Scalar element (with optional ConvertNode wrap):
    ///       d eq 'HR'  or  Convert(d) eq 'HR'   → ("value", "=", "HR")
    ///   - Property access on range variable:
    ///       a/Key eq 'Dept'                       → ("Key",   "=", "Dept")
    ///   - And-chain:
    ///       a/Key eq 'X' and a/Value eq 'Y'       → two entries
    ///
    /// The OData parser may wrap the range-variable reference in a ConvertNode
    /// (e.g. d eq 'HR' → Left.Kind == Convert → Source.Kind == NonResourceRangeVariableReference).
    /// We always unwrap Convert before inspecting the node kind.
    /// Range variable identity is determined by node kind, not by name — the name parameter
    /// is therefore not needed and intentionally omitted.
    /// </summary>
    private static IReadOnlyList<(string Column, string Op, object Value)> CollectAnyConditions(
        BinaryOperatorNode node)
    {
        var result = new List<(string, string, object)>();

        // Recurse into and-chain
        if (node.OperatorKind == BinaryOperatorKind.And)
        {
            result.AddRange(CollectAnyConditions((BinaryOperatorNode)node.Left));
            result.AddRange(CollectAnyConditions((BinaryOperatorNode)node.Right));
            return result;
        }

        var op = GetOperatorString(node.OperatorKind);
        var value = node.Right.GetConstantValue();

        // Unwrap ConvertNode — the OData parser often wraps the range variable
        // in a type-cast Convert node before handing it to the visitor
        var leftNode = node.Left;
        if (leftNode.Kind == QueryNodeKind.Convert)
        {
            leftNode = ((ConvertNode)leftNode).Source;
        }

        // Scalar element reference: d eq 'HR'
        if (leftNode.Kind == QueryNodeKind.NonResourceRangeVariableReference)
        {
            result.Add(("value", op, value));
            return result;
        }

        // Property access on the range variable: a/Key eq 'Dept'
        if (leftNode.Kind == QueryNodeKind.SingleValueOpenPropertyAccess)
        {
            var prop = ((SingleValueOpenPropertyAccessNode)leftNode).Name;
            result.Add((prop, op, value));
            return result;
        }

        throw new QueryException(
            $"Unsupported any() lambda left-hand side node kind: {leftNode.Kind}");
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
