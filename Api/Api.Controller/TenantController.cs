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
[ApiControllerName("Tenants")]
[Route("api/tenants")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.Tenant)]
public abstract class TenantController : RepositoryRootObjectController<ITenantService, ITenantRepository,
    Tenant, ApiObject.Tenant>
{
    protected IRegulationService RegulationService { get; }
    protected IRegulationShareService RegulationShareService { get; }
    protected IReportService ReportService { get; }

    protected TenantController(ITenantService tenantService, IRegulationService regulationService,
        IRegulationShareService regulationShareService,
        IReportService reportService, IControllerRuntime runtime) :
        base(tenantService, runtime, new TenantMap())
    {
        RegulationService = regulationService ?? throw new ArgumentNullException(nameof(regulationService));
        RegulationShareService = regulationShareService ?? throw new ArgumentNullException(nameof(regulationShareService));
        ReportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
    }

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
                var regulation = await RegulationService.GetAsync(Runtime.DbContext,permission.ProviderTenantId, permission.ProviderRegulationId);
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
        int tenantId, string methodName, Language? language, Dictionary<string, string> parameters = null)
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
            var dataTable = await ReportService.ExecuteQueryAsync(tenant, methodName, language,
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

    /// <summary>
    /// Get tenant calendar period
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="calculationMode">The calculation mode</param>
    /// <param name="periodMoment">The period evaluation date</param>
    /// <param name="calendar">The calendar configuration</param>
    /// <param name="culture">The culture to use</param>
    /// <param name="offset">The offset:<br />
    /// less than zero: past<br />
    /// zero: current<br />
    /// greater than zero: future<br /></param>
    /// <returns>The calendar period</returns>
    protected virtual async Task<ActionResult<DatePeriod>> GetCalendarPeriod(int tenantId,
        CalendarCalculationMode calculationMode,
        DateTime? periodMoment = null, CalendarConfiguration calendar = null, string culture = null, int? offset = null)
    {
        try
        {
            // tenant specific values
            if (calendar == null || string.IsNullOrWhiteSpace(culture))
            {
                var tenant = await Service.GetAsync(Runtime.DbContext, tenantId);
                // tenant culture
                if (string.IsNullOrWhiteSpace(culture))
                {
                    culture = tenant.Culture;
                }
                // tenant calendar
                calendar ??= tenant.Calendar;
            }

            // culture
            var cultureInfo = NewCultureInfo(culture);
            if (cultureInfo == null)
            {
                return BadRequest($"Unknown culture: {culture}");
            }

            // calendar
            if (calendar == null)
            {
                return BadRequest("Missing calendar");
            }

            // calculator
            var calculator = PayrollCalculatorFactory.CreateCalculator(
                calculationMode: calculationMode,
                calendar: calendar,
                tenantId: tenantId,
                culture: cultureInfo);
            periodMoment ??= CurrentEvaluationDate;
            var period = calculator.GetPayrunPeriod(periodMoment.Value);

            // offset period
            offset ??= 0;
            period = period.GetPayrollPeriod(period.Start, offset.Value);
            return new DatePeriod(period.Start, period.End);
        }
        catch (Exception exception)
        {
            Log.Error(exception, exception.GetBaseMessage());
            return InternalServerError(exception);
        }
    }

    protected virtual async Task<ActionResult<DatePeriod>> GetCalendarCycle(
        int tenantId, CalendarCalculationMode calculationMode,
        DateTime? cycleMoment = null, CalendarConfiguration calendar = null, string culture = null, int? offset = null)
    {
        try
        {
            // tenant specific values
            if (calendar == null || string.IsNullOrWhiteSpace(culture))
            {
                var tenant = await Service.GetAsync(Runtime.DbContext, tenantId);
                // tenant culture
                if (string.IsNullOrWhiteSpace(culture))
                {
                    culture = tenant.Culture;
                }
                // tenant calendar
                calendar ??= tenant.Calendar;
            }

            // culture
            var cultureInfo = NewCultureInfo(culture);
            if (cultureInfo == null)
            {
                return BadRequest($"Unknown culture: {culture}");
            }

            // calendar
            if (calendar == null)
            {
                return BadRequest("Missing calendar");
            }

            // calculator
            var calculator = PayrollCalculatorFactory.CreateCalculator(
                calculationMode: calculationMode,
                tenantId: tenantId,
                calendar: calendar,
                culture: cultureInfo);
            cycleMoment ??= CurrentEvaluationDate;
            var period = calculator.GetPayrunCycle(cycleMoment.Value);

            // offset period
            offset ??= 0;
            period = period.GetPayrollPeriod(period.Start, offset.Value);
            return new DatePeriod(period.Start, period.End);
        }
        catch (Exception exception)
        {
            Log.Error(exception, exception.GetBaseMessage());
            return InternalServerError(exception);
        }
    }

    protected virtual async Task<ActionResult<decimal?>> CalculateCalendarValue(
        int tenantId, CalendarCalculationMode calculationMode,
        decimal value, DateTime? evaluationDate = null, DateTime? evaluationPeriodDate = null,
        CalendarConfiguration calendar = null, string culture = null)
    {
        try
        {
            // tenant specific values
            if (calendar == null || string.IsNullOrWhiteSpace(culture))
            {
                var tenant = await Service.GetAsync(Runtime.DbContext, tenantId);
                // tenant culture
                if (string.IsNullOrWhiteSpace(culture))
                {
                    culture = tenant.Culture;
                }
                // tenant calendar
                calendar ??= tenant.Calendar;
            }

            // culture
            var cultureInfo = NewCultureInfo(culture);
            if (cultureInfo == null)
            {
                return BadRequest($"Unknown culture: {culture}");
            }

            // calendar
            if (calendar == null)
            {
                return BadRequest("Missing calendar");
            }

            // calculator
            var calculator = PayrollCalculatorFactory.CreateCalculator(
                calculationMode: calculationMode,
                tenantId: tenantId,
                calendar: calendar,
                culture: cultureInfo);
            evaluationDate ??= CurrentEvaluationDate;
            evaluationPeriodDate ??= evaluationDate;
            var evaluationPayrollPeriod = calculator.GetPayrunPeriod(evaluationPeriodDate.Value);
            var evaluationPeriod = new DatePeriod(evaluationPayrollPeriod.Start, evaluationPayrollPeriod.End);

            var calculation = new CaseValueCalculation
            {
                EvaluationDate = evaluationDate.Value,
                EvaluationPeriod = evaluationPeriod,
                // currently no custom case value period supported
                CaseValuePeriod = evaluationPeriod,
                CaseValue = value
            };
            return calculator.CalculateCasePeriodValue(calculation);
        }
        catch (Exception exception)
        {
            Log.Error(exception, exception.GetBaseMessage());
            return InternalServerError(exception);
        }
    }
}