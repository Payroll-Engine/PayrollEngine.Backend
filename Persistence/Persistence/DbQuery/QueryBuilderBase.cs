using System;
using System.Collections.Generic;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.OData.UriParser.Aggregation;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence.DbQuery;

/// <summary>
/// Sql query builder with partial OData support
/// </summary>
internal abstract class QueryBuilderBase
{
    /// <summary>
    /// The query context
    /// </summary>
    protected abstract IQueryContext QueryContext { get; }

    /// <summary>
    /// Builds a SQL query from a domain query
    /// </summary>
    /// <param name="tableName">Name of the table</param>
    /// <param name="query">The domain object query</param>
    /// <param name="queryMode">The query mode</param>
    /// <returns>The SQL query</returns>
    internal SqlKata.Query BuildQuery(string tableName, Query query = null, QueryMode queryMode = QueryMode.Item)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException(nameof(tableName));
        }

        var queryOptions = new Dictionary<string, string>();
        if (query != null)
        {
            // status
            if (query.Status.HasValue)
            {
                queryOptions.Add(DbSchema.ObjectColumn.Status, query.Status.Value.ToString());
            }

            // order by
            if (!string.IsNullOrWhiteSpace(query.OrderBy))
            {
                queryOptions.Add(QuerySpecification.OrderByOperation, query.OrderBy);
            }

            // filter
            if (!string.IsNullOrWhiteSpace(query.Filter))
            {
                queryOptions.Add(QuerySpecification.FilterOperation, query.Filter);
            }

            // select
            if (!string.IsNullOrWhiteSpace(query.Select))
            {
                queryOptions.Add(QuerySpecification.SelectOperation, query.Select);
            }

            // ignore paging in count mode
            if (queryMode == QueryMode.Item)
            {
                // top
                if (query.Top.HasValue)
                {
                    queryOptions.Add(QuerySpecification.TopOperation, query.Top.Value.ToString());
                }

                // skip
                if (query.Skip.HasValue)
                {
                    queryOptions.Add(QuerySpecification.SkipOperation, query.Skip.Value.ToString());
                }
            }
        }

        // build query
        return BuildQuery(tableName, queryOptions, queryMode);
    }

    /// <summary>
    /// Builds a SQL query from query option (name/value dictionary)
    /// </summary>
    /// <param name="tableName">Name of the table</param>
    /// <param name="queryOptions">The dictionary storing query option key-value pairs</param>
    /// <param name="queryMode">The query mode</param>
    /// <returns>Tuple with sql string and arguments dictionary</returns>
    private SqlKata.Query BuildQuery(string tableName, IDictionary<string, string> queryOptions, QueryMode queryMode = QueryMode.Item)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException(nameof(tableName));
        }
        if (queryOptions == null)
        {
            throw new ArgumentNullException(nameof(queryOptions));
        }

        // OData parser
        var result = BuildTableModel(tableName);
        var model = result.Item1;
        var entityType = result.Item2;
        var entitySet = result.Item3;
        var parser = new ODataQueryOptionParser(model, entityType, entitySet, queryOptions)
        {
            Resolver =
            {
                EnableCaseInsensitive = true,
                EnableNoDollarQueryOptions = true
            }
        };

        // parse apply
        ApplyClause applyClause;
        try
        {
            applyClause = parser.ParseApply();
        }
        catch (ODataException exception)
        {
            throw new QueryException($"Query apply error: {exception.GetBaseMessage()}", exception);
        }

        // parse filter
        FilterClause filterClause;
        try
        {
            filterClause = parser.ParseFilter();
        }
        catch (ODataException exception)
        {
            throw new QueryException($"Query filter error: {exception.GetBaseMessage()}", exception);
        }

        // paging
        long? top = null;
        long? skip = null;
        // ignore paging in count mode
        if (queryMode == QueryMode.Item)
        {
            // parse top
            try
            {
                top = parser.ParseTop();
            }
            catch (ODataException exception)
            {
                throw new QueryException($"Query top error: {exception.GetBaseMessage()}", exception);
            }

            // parse skip
            try
            {
                skip = parser.ParseSkip();
            }
            catch (ODataException exception)
            {
                throw new QueryException($"Query skip error: {exception.GetBaseMessage()}", exception);
            }
        }

        // parse order by
        OrderByClause orderByClause;
        try
        {
            orderByClause = parser.ParseOrderBy();
        }
        catch (ODataException exception)
        {
            throw new QueryException($"Query order by error: {exception.GetBaseMessage()}", exception);
        }

        // parse select
        SelectExpandClause selectClause;
        try
        {
            selectClause = parser.ParseSelectAndExpand();
            // ensure object id is always selected
            if (selectClause != null && !selectClause.AllSelected)
            {
                var hasId = false;
                foreach (var selectedItem in selectClause.SelectedItems)
                {
                    if (selectedItem is not PathSelectItem pathSelectItem)
                    {
                        continue;
                    }
                    var identifier = pathSelectItem.SelectedPath.FirstSegment.Identifier;
                    if (!string.Equals(identifier, nameof(IDomainObject.Id), StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }
                    hasId = true;
                    break;
                }
                if (!hasId)
                {
                    throw new QueryException($"Query select error: missing {nameof(IDomainObject.Id)}");
                }
            }
        }
        catch (ODataException exception)
        {
            throw new QueryException($"Query select error: {exception.GetBaseMessage()}", exception);
        }

        // target query
        var query = new SqlKata.Query(tableName);

        // count
        if (queryMode == QueryMode.ItemCount)
        {
            query = query.AsCount();
        }

        // top
        if (top.HasValue)
        {
            query = query.Take(Convert.ToInt32(top.Value));
        }
        // skip
        if (skip.HasValue)
        {
            query = query.Skip(Convert.ToInt32(skip.Value));
        }

        // apply
        if (applyClause != null)
        {
            query = new ApplyClauseBuilder(QueryContext).BuildApplyClause(query, applyClause);
            if (filterClause != null || selectClause != null)
            {
                query = new SqlKata.Query().From(query);
            }
        }

        // filter
        if (filterClause != null)
        {
            query = filterClause.Expression.Accept(new FilterClauseBuilder(query, QueryContext));
        }

        if (queryMode == QueryMode.Item)
        {
            // order by
            if (orderByClause != null)
            {
                query = BuildOrderByClause(query, orderByClause);
            }
            // select
            if (selectClause != null)
            {
                query = BuildSelectClause(query, selectClause);
            }
        }

        return query;
    }

    /// <summary>
    /// Builds the order by clause
    /// </summary>
    /// <param name="query">The SQL query</param>
    /// <param name="orderByClause">The order by clause</param>
    /// <returns>Updated query</returns>
    private SqlKata.Query BuildOrderByClause(SqlKata.Query query, OrderByClause orderByClause)
    {
        while (orderByClause != null)
        {
            var direction = orderByClause.Direction;
            if (orderByClause.Expression is SingleValueOpenPropertyAccessNode expression)
            {
                var column = expression.GetColumnName(QueryContext);
                query = direction == OrderByDirection.Ascending ?
                    query.OrderBy(column) :
                    query.OrderByDesc(column);
            }

            orderByClause = orderByClause.ThenBy;
        }

        return query;
    }

    /// <summary>
    /// Builds the select clause
    /// </summary>
    /// <param name="query">The SQL query</param>
    /// <param name="selectClause">The select clause</param>
    /// <returns>Updated query</returns>
    private SqlKata.Query BuildSelectClause(SqlKata.Query query, SelectExpandClause selectClause)
    {
        if (!selectClause.AllSelected)
        {
            foreach (var selectItem in selectClause.SelectedItems)
            {
                if (selectItem is PathSelectItem path)
                {
                    var column = QueryContext.ValidateColumn(path.SelectedPath.FirstSegment.Identifier);
                    query = query.Select(column);
                }
            }
        }
        return query;
    }

    /// <summary>
    /// Builds the table model
    /// </summary>
    /// <param name="tableName">Name of the table</param>
    private static (IEdmModel, IEdmEntityType, IEdmEntitySet) BuildTableModel(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentNullException(nameof(tableName));
        }

        const string defaultNamespace = "QueryBuilder";

        // model
        var model = new EdmModel();
        var entityType = new EdmEntityType(defaultNamespace, tableName, null, false, true);
        model.AddElement(entityType);

        // container
        var defaultContainer = new EdmEntityContainer(defaultNamespace, "DefaultContainer");
        model.AddElement(defaultContainer);
        var entitySet = defaultContainer.AddEntitySet(tableName, entityType);

        return (model, entityType, entitySet);
    }
}