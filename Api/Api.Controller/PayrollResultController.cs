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
    /// <summary>
    /// Query payroll result values />
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="employeeId">The employee id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The payroll result values, count or both</returns>
    public virtual async Task<ActionResult> QueryPayrollResultValuesAsync(int tenantId, int? employeeId,
        Query query)
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
                    var items = await QueryResultValuesAsync(tenantId, employeeId, query);
                    return items.IsValidResult() ? Ok(items.Value) : items.Result;
                case QueryResultType.Count:
                    var count = await QueryResultValueCountAsync(tenantId, employeeId, query);
                    return count.IsValidResult() ? Ok(count.Value) : count.Result;
                case QueryResultType.ItemsWithCount:
                    items = await QueryResultValuesAsync(tenantId, employeeId, query);
                    count = await QueryResultValueCountAsync(tenantId, employeeId, query);
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

    /// <summary>
    /// Query payroll result values
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="employeeId">The employee id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>List of requested Api objects</returns>
    private async Task<ActionResult<ApiObject.PayrollResultValue[]>> QueryResultValuesAsync(int tenantId, int? employeeId = null, Query query = null)
    {
        try
        {
            var apiObjects = new List<ApiObject.PayrollResultValue>();
            var items = (await ChildService.QueryResultValuesAsync(Runtime.DbContext, tenantId, employeeId, query)).ToList();

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
    /// Count count of payroll result values
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="employeeId">The employee id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>Count of requested Api objects</returns>
    private async Task<ActionResult<long>> QueryResultValueCountAsync(int tenantId, int? employeeId = null, Query query = null)
    {
        try
        {
            return await Service.QueryResultValueCountAsync(Runtime.DbContext, tenantId, employeeId, query);
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
}