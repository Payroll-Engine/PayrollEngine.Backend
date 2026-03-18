using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the payroll results
/// </summary>
public abstract class PayrollResultController(ITenantService tenantService, IPayrollResultService payrollResultService,
        IControllerRuntime runtime)
    : RepositoryChildObjectController<ITenantService, IPayrollResultService,
    ITenantRepository, IPayrollResultRepository,
    Tenant, PayrollResult, ApiObject.PayrollResult>(tenantService, payrollResultService, runtime, new PayrollResultMap())
{
    #region Collector Results

    /// <summary>
    /// Query payroll collector results
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollResultId">The payroll result id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The payroll collector results</returns>
    public virtual async Task<ActionResult<ApiObject.CollectorResult[]>> QueryCollectorResultsAsync(
        int tenantId, int payrollResultId, Query query)
    {
        try
        {
            var results = await Service.QueryCollectorResultsAsync(Runtime.DbContext, payrollResultId, query);
            return new CollectorResultMap().ToApi(results);
        }
        catch (QueryException exception)
        {
            return QueryError(exception);
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    /// <summary>
    /// Query payroll collector custom results
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollResultId">The payroll result id</param>
    /// <param name="collectorResultId">The collector result id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The payroll collector custom results</returns>
    public virtual async Task<ActionResult<ApiObject.CollectorCustomResult[]>> QueryCollectorCustomResultsAsync(
        int tenantId, int payrollResultId, int collectorResultId, Query query)
    {
        try
        {
            var results = await Service.QueryCollectorCustomResultsAsync(Runtime.DbContext, collectorResultId, query);
            return new CollectorCustomResultMap().ToApi(results);
        }
        catch (QueryException exception)
        {
            return QueryError(exception);
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    #endregion

    #region Wage Type Results

    /// <summary>
    /// Query payroll wage type results
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollResultId">The payroll result id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The payroll wage type results</returns>
    public virtual async Task<ActionResult<ApiObject.WageTypeResult[]>> QueryWageTypeResultsAsync(
        int tenantId, int payrollResultId, Query query)
    {
        try
        {
            var results = await Service.QueryWageTypeResultsAsync(Runtime.DbContext, payrollResultId, query);
            return new WageTypeResultMap().ToApi(results);
        }
        catch (QueryException exception)
        {
            return QueryError(exception);
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    /// <summary>
    /// Query payroll wage type custom results
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollResultId">The payroll result id</param>
    /// <param name="wageTypeResultId">The wage type result id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The payroll wage type custom results</returns>
    public virtual async Task<ActionResult<ApiObject.WageTypeCustomResult[]>> QueryWageTypeCustomResultsAsync(
        int tenantId, int payrollResultId, int wageTypeResultId, Query query)
    {
        try
        {
            var results = await Service.QueryWageTypeCustomResultsAsync(Runtime.DbContext, wageTypeResultId, query);
            return new WageTypeCustomResultMap().ToApi(results);
        }
        catch (QueryException exception)
        {
            return QueryError(exception);
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    #endregion

    #region Payrun Results

    /// <summary>
    /// Query payroll payrun results
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollResultId">The payroll result id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The payroll payrun results</returns>
    public virtual async Task<ActionResult<ApiObject.PayrunResult[]>> QueryPayrunResultsAsync(
        int tenantId, int payrollResultId, Query query)
    {
        try
        {
            var results = await Service.QueryPayrunResultsAsync(Runtime.DbContext, payrollResultId, query);
            return new PayrunResultMap().ToApi(results);
        }
        catch (QueryException exception)
        {
            return QueryError(exception);
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    #endregion

    #region Result Values

    /// <summary>
    /// Query payroll result values />
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="employeeId">The employee id</param>
    /// <param name="divisionId">The division id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The payroll result values, count or both</returns>
    public virtual async Task<ActionResult> QueryPayrollResultValuesAsync(int tenantId, int? employeeId,
        int? divisionId, Query query)
    {
        try
        {
            // tenant check
            if (!await ParentService.ExistsAsync(Runtime.DbContext, tenantId))
            {
                return NotFound(typeof(Tenant), tenantId);
            }

            query ??= new();
            query.Result ??= QueryResultType.Items;
            switch (query.Result)
            {
                case QueryResultType.Items:
                    var items = await QueryResultValuesAsync(tenantId, employeeId, divisionId, query);
                    return items.IsValidResult() ? Ok(items.Value) : items.Result;
                case QueryResultType.Count:
                    var count = await QueryResultValueCountAsync(tenantId, employeeId, divisionId, query);
                    return count.IsValidResult() ? Ok(count.Value) : count.Result;
                case QueryResultType.ItemsWithCount:
                    items = await QueryResultValuesAsync(tenantId, employeeId, divisionId, query);
                    count = await QueryResultValueCountAsync(tenantId, employeeId, divisionId, query);
                    return items.IsValidResult() && count.IsValidResult() ?
                        Ok(new QueryResult<ApiObject.PayrollResultValue>(items.Value, count.Value)) : items.Result;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    #endregion

    #region Result Sets

    /// <summary>
    /// Query payroll result sets
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The payroll result sets</returns>
    public virtual async Task<ActionResult<ApiObject.PayrollResultSet[]>> QueryPayrollResultSetsAsync(
        int tenantId, Query query)
    {
        try
        {
            var resultSets = await Service.QueryResultSetsAsync(Runtime.DbContext, tenantId, query);
            return new PayrollResultSetMap().ToApi(resultSets);
        }
        catch (QueryException exception)
        {
            return QueryError(exception);
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    /// <summary>
    /// Get a payroll result set
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollResultId">The payroll result id</param>
    /// <returns>A payroll result set</returns>
    public virtual async Task<ActionResult<ApiObject.PayrollResultSet>> GetPayrollResultSetAsync(
        int tenantId, int payrollResultId)
    {
        try
        {
            var resultSet = await Service.GetResultSetAsync(Runtime.DbContext, tenantId, payrollResultId);
            if (resultSet == null)
            {
                return NotFound($"Payroll result with id {payrollResultId} was not found");
            }
            return new PayrollResultSetMap().ToApi(resultSet);
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    #endregion

    #region Private

    /// <summary>
    /// Query payroll result values
    /// </summary>
    private async Task<ActionResult<ApiObject.PayrollResultValue[]>> QueryResultValuesAsync(int tenantId, int? employeeId = null, int? divisionId = null, Query query = null)
    {
        try
        {
            var apiObjects = new List<ApiObject.PayrollResultValue>();
            var items = (await ChildService.QueryResultValuesAsync(Runtime.DbContext, tenantId, employeeId, divisionId, query)).ToList();

            var map = new PayrollResultValueMap();
            foreach (var item in items)
            {
                apiObjects.Add(map.ToApi(item));
            }
            return apiObjects.ToArray();
        }
        catch (QueryException exception)
        {
            return QueryError(exception);
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    /// <summary>
    /// Count of payroll result values
    /// </summary>
    private async Task<ActionResult<long>> QueryResultValueCountAsync(int tenantId, int? employeeId = null, int? divisionId = null, Query query = null)
    {
        try
        {
            return await Service.QueryResultValueCountAsync(Runtime.DbContext, tenantId, employeeId, divisionId, query);
        }
        catch (QueryException exception)
        {
            return QueryError(exception);
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    #endregion
}