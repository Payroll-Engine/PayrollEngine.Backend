using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using PayrollEngine.Domain.Model;
using PayrollEngine.Serialization;
using PayrollEngine.Persistence.DbQuery;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public abstract class CaseValueRepositoryBase<TDomain>(string tableName, string parentFieldName,
        ICaseFieldRepository caseFieldRepository)
    : ChildDomainRepository<TDomain>(tableName, parentFieldName), ICaseValueRepository<TDomain>
    where TDomain : CaseValue, new()
{
    private ICaseFieldRepository CaseFieldRepository { get; } = caseFieldRepository ?? throw new ArgumentNullException(nameof(caseFieldRepository));

    /// <summary>The table name</summary>
    protected abstract string CaseValueTableName { get; }
    /// <summary>The query stored procedure</summary>
    protected abstract string CaseValueQueryProcedure { get; }

    protected override void GetCreateData(int parentId, TDomain caseValue, DbParameterCollection parameters)
    {
        // test value period
        if (!caseValue.Start.HasValue && caseValue.End.HasValue)
        {
            throw new PayrollException("Case value period without start date.");
        }
        if (caseValue.Start.HasValue && caseValue.End.HasValue &&
            caseValue.End < caseValue.Start)
        {
            throw new PayrollException("Case value period end date before start date.");
        }

        // parent field
        parameters.Add(ParentFieldName, parentId, DbType.Int32);

        // case value fields
        parameters.Add(nameof(caseValue.DivisionId), caseValue.DivisionId, DbType.Int32);
        parameters.Add(nameof(caseValue.CaseName), caseValue.CaseName);
        parameters.Add(nameof(caseValue.CaseNameLocalizations), JsonSerializer.SerializeNamedDictionary(caseValue.CaseNameLocalizations));
        parameters.Add(nameof(caseValue.CaseFieldName), caseValue.CaseFieldName);
        parameters.Add(nameof(caseValue.CaseFieldNameLocalizations), JsonSerializer.SerializeNamedDictionary(caseValue.CaseFieldNameLocalizations));
        parameters.Add(nameof(caseValue.CaseSlot), caseValue.CaseSlot);
        parameters.Add(nameof(caseValue.CaseSlotLocalizations), JsonSerializer.SerializeNamedDictionary(caseValue.CaseSlotLocalizations));
        parameters.Add(nameof(caseValue.ValueType), caseValue.ValueType, DbType.Int32);
        parameters.Add(nameof(caseValue.Value), caseValue.Value);
        parameters.Add(nameof(caseValue.NumericValue), caseValue.NumericValue, DbType.Decimal);
        parameters.Add(nameof(caseValue.Culture), caseValue.Culture);
        parameters.Add(nameof(caseValue.CaseRelation), caseValue.CaseRelation);
        parameters.Add(nameof(caseValue.CancellationDate), caseValue.CancellationDate, DbType.DateTime2);
        parameters.Add(nameof(caseValue.Start), caseValue.Start, DbType.DateTime2);
        parameters.Add(nameof(caseValue.End), caseValue.End, DbType.DateTime2);
        parameters.Add(nameof(caseValue.Forecast), caseValue.Forecast);
        parameters.Add(nameof(caseValue.Tags), JsonSerializer.SerializeList(caseValue.Tags));
        parameters.Add(nameof(caseValue.Attributes), JsonSerializer.SerializeNamedDictionary(caseValue.Attributes));
        base.GetCreateData(parentId, caseValue, parameters);
    }

    #region Query

    /// <inheritdoc />
    /// <remarks>Do not call the base class method</remarks>
    public override async Task<IEnumerable<TDomain>> QueryAsync(IDbContext context, int parentId, Query query = null)
    {
        // db query to support case value attributes
        var dbQuery = DbQueryFactory.NewTypeQuery<TDomain>(CaseValueTableName, ParentFieldName,
            parentId, query);

        // query compilation
        var compileQuery = CompileQuery(dbQuery.Item1);

        // execute stored procedure
        var items = (await QueryCaseValuesAsync<TDomain>(context,
            new()
            {
                ParentId = parentId,
                StoredProcedure = CaseValueQueryProcedure,
                Query = compileQuery,
                QueryAttributes = dbQuery.Item2
            })).ToList();

        // notification
        await OnRetrieved(context, parentId, items);

        return items;
    }

    /// <inheritdoc />
    /// <remarks>Do not call the base class method</remarks>
    public override async Task<long> QueryCountAsync(IDbContext context, int parentId, Query query = null)
    {
        // db query to support case value attributes
        var dbQuery = DbQueryFactory.NewTypeQuery<TDomain>(CaseValueTableName, ParentFieldName,
            parentId, query, queryMode: QueryMode.ItemCount);
        SetupDbQuery(dbQuery.Item1, query);

        // query compilation
        var compileQuery = CompileQuery(dbQuery.Item1);

        // execute stored procedure
        var count = await QueryCaseValueCountAsync(context,
            new()
            {
                ParentId = parentId,
                StoredProcedure = CaseValueQueryProcedure,
                Query = compileQuery,
                QueryAttributes = dbQuery.Item2
            });
        return count;
    }

    protected override void SetupDbQuery(SqlKata.Query dbQuery, Query query)
    {
        base.SetupDbQuery(dbQuery, query);

        // case value query
        if (query is Domain.Model.CaseValueQuery caseValueQuery)
        {
            caseValueQuery.ApplyTo(dbQuery);
        }
    }

    #endregion

    public async Task<IEnumerable<string>> GetCaseValueSlotsAsync(IDbContext context, int parentId, string caseFieldName)
    {
        if (parentId <= 0)
        {
            throw new ArgumentException(nameof(parentId));
        }
        // filter by case field name
        if (string.IsNullOrWhiteSpace(caseFieldName))
        {
            throw new ArgumentException(nameof(caseFieldName));
        }

        // query
        var query = DbQueryFactory.NewQuery(TableName, ParentFieldName, parentId);

        // filter and group by case field name
        query.Select(nameof(CaseValue.CaseSlot));
        query.Where(DbSchema.CaseValueColumn.CaseFieldName, caseFieldName);
        query.GroupBy(nameof(CaseValue.CaseSlot));

        // compile and execute query
        var compileQuery = CompileQuery(query);
        var caseValues = await QueryAsync<CaseValue>(context, compileQuery);

        // select case slots
        return caseValues.Select(x => x.CaseSlot);
    }

    public async Task<IEnumerable<CaseValue>> GetCaseValuesAsync(IDbContext context, DomainCaseValueQuery query,
        string caseFieldName = null, DateTime? evaluationDate = null)
    {
        if (query.ParentId <= 0)
        {
            throw new ArgumentException(nameof(query.ParentId));
        }
        if (evaluationDate.HasValue && !evaluationDate.Value.IsUtc())
        {
            throw new ArgumentException("Case value query moment must be UTC.", nameof(evaluationDate));
        }

        // query
        var dbQuery = DbQueryFactory.NewQuery(TableName, ParentFieldName, query.ParentId);

        // filter by division
        dbQuery.AddDivisionFilter(query.DivisionScope, query.DivisionId);

        // filter by case field name
        if (!string.IsNullOrWhiteSpace(caseFieldName))
        {
            dbQuery.Where(DbSchema.CaseValueColumn.CaseFieldName, caseFieldName);
        }

        // forecast filter
        if (string.IsNullOrWhiteSpace(query.Forecast))
        {
            // only results without forecast
            dbQuery.WhereNull(DbSchema.CaseValueColumn.Forecast);
        }
        else
        {
            // specific forecast results plus results without forecast
            dbQuery.WhereNullOrValue(DbSchema.CaseValueColumn.Forecast, query.Forecast);
        }

        // ignore newer created objects
        if (evaluationDate.HasValue)
        {
            dbQuery.Where(DbSchema.ObjectColumn.Created, "<", evaluationDate);
        }

        // order from newest to oldest
        dbQuery.OrderBy(DbSchema.ObjectColumn.Created);
        var compileQuery = CompileQuery(dbQuery);
        return await QueryAsync<CaseValue>(context, compileQuery);
    }

    public async Task<IEnumerable<CaseValue>> GetPeriodCaseValuesAsync(IDbContext context, DomainCaseValueQuery query,
        DatePeriod period, string caseFieldName = null, DateTime? evaluationDate = null)
    {
        if (query.ParentId <= 0)
        {
            throw new ArgumentException(nameof(query.ParentId));
        }
        if (!period.IsUtc)
        {
            throw new ArgumentException("Case value query period must be UTC.", nameof(period));
        }
        if (evaluationDate.HasValue && !evaluationDate.Value.IsUtc())
        {
            throw new ArgumentException("Case value query moment must be UTC.", nameof(evaluationDate));
        }

        // query
        var dbQuery = DbQueryFactory.NewQuery(TableName, ParentFieldName, query.ParentId);

        // filter by division
        dbQuery.AddDivisionFilter(query.DivisionScope, query.DivisionId);

        // filter by case field name
        if (!string.IsNullOrWhiteSpace(caseFieldName))
        {
            dbQuery.Where(DbSchema.CaseValueColumn.CaseFieldName, caseFieldName);
        }

        // forecast filter
        if (string.IsNullOrWhiteSpace(query.Forecast))
        {
            // only results without forecast
            dbQuery.WhereNull(DbSchema.CaseValueColumn.Forecast);
        }
        else
        {
            // specific forecast results plus results without forecast
            dbQuery.WhereNullOrValue(DbSchema.CaseValueColumn.Forecast, query.Forecast);
        }

        // ignore newer created objects
        if (evaluationDate.HasValue)
        {
            dbQuery.Where(DbSchema.ObjectColumn.Created, "<", evaluationDate);
        }

        // period filter: ignore values from outside periods
        // sub-conditions see
        // https://stackoverflow.com/questions/50590851/how-to-join-multiple-where-clauses-together-in-sqlkata
        if (period.HasEnd)
        {
            // sub condition: ([Created] IS NULL OR [Created] < 'periodEnd')
            dbQuery.WhereNullOrValue(DbSchema.CaseValueColumn.Start, "<", period.End);
        }
        if (period.HasStart)
        {
            // sub condition: ([Created] IS NULL OR [Created] > 'periodStart')
            dbQuery.WhereNullOrValue(DbSchema.CaseValueColumn.End, ">", period.Start);
        }

        // order from newest to oldest
        dbQuery.OrderByDesc(DbSchema.ObjectColumn.Created);
        var compileQuery = CompileQuery(dbQuery);
        return await QueryAsync<CaseValue>(context, compileQuery);
    }

    public async Task<CaseValue> GetRetroCaseValueAsync(IDbContext context, DomainCaseValueQuery query,
        DatePeriod period, string caseFieldName)
    {
        if (query.ParentId <= 0)
        {
            throw new ArgumentException(nameof(query.ParentId));
        }
        if (!period.IsUtc)
        {
            throw new ArgumentException("Case value query period must be UTC.", nameof(period));
        }
        if (string.IsNullOrWhiteSpace(caseFieldName))
        {
            throw new ArgumentException(nameof(caseFieldName));
        }

        // query
        var dbQuery = DbQueryFactory.NewQuery(TableName, ParentFieldName, query.ParentId);

        // filter by division
        dbQuery.AddDivisionFilter(query.DivisionScope, query.DivisionId);

        // filter by case field name
        dbQuery.Where(DbSchema.CaseValueColumn.CaseFieldName, caseFieldName);

        // forecast filter
        if (string.IsNullOrWhiteSpace(query.Forecast))
        {
            // only results without forecast
            dbQuery.WhereNull(DbSchema.CaseValueColumn.Forecast);
        }
        else
        {
            // specific forecast results plus results without forecast
            dbQuery.WhereNullOrValue(DbSchema.CaseValueColumn.Forecast, query.Forecast);
        }

        // filter case values created or cancelled within the requested time period
        dbQuery.Where(q => q.WhereBetween(DbSchema.ObjectColumn.Created, period.Start, period.End)
            .OrWhereBetween(DbSchema.CaseChangeColumn.CancellationDate, period.Start, period.End));

        // order from newest to oldest
        dbQuery.OrderByDesc(DbSchema.ObjectColumn.Created);
        // retrieve case values
        var compileQuery = CompileQuery(dbQuery);
        var caseValues = await QueryAsync<CaseValue>(context, compileQuery);
        // return the oldest created case value
        return caseValues.MinBy(x => x.Start);
    }

    public override async Task<TDomain> CreateAsync(IDbContext context, int parentId, TDomain item)
    {
        // check for valid case field name
        if (string.IsNullOrWhiteSpace(item.CaseFieldName))
        {
            throw new PayrollException("Missing case field name.");
        }
        if (!await CaseFieldRepository.ExistsAsync(context, DbSchema.CaseFieldColumn.Name, item.CaseFieldName))
        {
            throw new PayrollException($"Unknown case field with name {item.CaseFieldName}.");
        }

        return await base.CreateAsync(context, parentId, item);
    }
}