using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Data;
using PayrollEngine.Domain.Model.Repository;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the payroll tenants
/// </summary>
public abstract class TenantController(ITenantService tenantService, IRegulationService regulationService,
        IRegulationShareService regulationShareService, IReportService reportService,
        IControllerRuntime runtime)
    : RepositoryRootObjectController<ITenantService, ITenantRepository,
    Tenant, ApiObject.Tenant>(tenantService, runtime, new TenantMap())
{
    private IRegulationService RegulationService { get; } = regulationService ?? throw new ArgumentNullException(nameof(regulationService));
    private IRegulationShareService RegulationShareService { get; } = regulationShareService ?? throw new ArgumentNullException(nameof(regulationShareService));
    private IReportService ReportService { get; } = reportService ?? throw new ArgumentNullException(nameof(reportService));

    public virtual async Task<ActionResult<IEnumerable<ApiObject.Regulation>>> GetSharedRegulationsAsync(
        int tenantId, int? divisionId)
    {
        try
        {
            // tenant
            var tenant = await Service.GetAsync(Runtime.DbContext, tenantId);
            if (tenant == null)
            {
                return BadRequest($"Unknown tenant with id {tenantId}");
            }

            // permissions
            var query = new Query
            {
                Status = ObjectStatus.Active,
                Filter = $"{nameof(RegulationShare.ConsumerTenantId)} eq {tenantId}"
            };
            if (divisionId.HasValue)
            {
                query.Filter += $" and ({nameof(RegulationShare.ConsumerDivisionId)} eq null or " +
                                $"{nameof(RegulationShare.ConsumerDivisionId)} eq {divisionId.Value})";
            }
            var permissions = await RegulationShareService.QueryAsync(Runtime.DbContext, query);

            // regulations
            var map = new RegulationMap();
            var regulations = new List<ApiObject.Regulation>();
            foreach (var permission in permissions)
            {
                var regulation = await RegulationService.GetAsync(Runtime.DbContext, permission.ProviderTenantId, permission.ProviderRegulationId);
                if (regulation != null)
                {
                    regulations.Add(map.ToApi(regulation));
                }
            }

            return regulations;
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

    public virtual async Task<ActionResult<DataTable>> ExecuteReportQueryAsync(
        int tenantId, string methodName, string culture, Dictionary<string, string> parameters = null)
    {
        try
        {
            // tenant
            var tenant = await Service.GetAsync(Runtime.DbContext, tenantId);
            if (tenant == null)
            {
                return BadRequest($"Unknown tenant with id {tenantId}");
            }

            // query
            var dataTable = await ReportService.ExecuteQueryAsync(tenant, methodName, culture,
                parameters, new ApiControllerContext(ControllerContext));
            return dataTable;
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

    public virtual async Task<ActionResult<IEnumerable<ApiObject.ActionInfo>>> GetSystemScriptActionsAsync(
        int tenantId, FunctionType functionType = FunctionType.All)
    {
        try
        {
            // tenant
            var tenant = await Service.GetAsync(Runtime.DbContext, tenantId);
            if (tenant == null)
            {
                return BadRequest($"Unknown tenant with id {tenantId}");
            }

            // system actions
            var actions = await Service.GetSystemScriptActionsAsync(functionType);
            return new ActionInfoMap().ToApi(actions);
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    public virtual async Task<ActionResult<IEnumerable<ApiObject.ActionInfo>>> GetSystemScriptActionPropertiesAsync(
        int tenantId, FunctionType functionType = FunctionType.All, bool readOnly = true)
    {
        try
        {
            // tenant
            var tenant = await Service.GetAsync(Runtime.DbContext, tenantId);
            if (tenant == null)
            {
                return BadRequest($"Unknown tenant with id {tenantId}");
            }

            // system actions
            var actions = await Service.GetSystemScriptActionPropertiesAsync(functionType, readOnly);
            return new ActionInfoMap().ToApi(actions);
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }
}