using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Model;
using System.Threading.Tasks;
using System;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for calendars
/// </summary>
public abstract class CalendarController(ITenantService tenantService, ICalendarService calendarService,
        IPayrollCalculatorProvider payrollCalculatorProvider, IControllerRuntime runtime)
    : RepositoryChildObjectController<ITenantService, ICalendarService,
    ITenantRepository, ICalendarRepository,
    Tenant, Calendar, ApiObject.Calendar>(tenantService, calendarService, runtime, new CalendarMap())
{
    private IPayrollCalculatorProvider PayrollCalculatorProvider { get; } = payrollCalculatorProvider ?? throw new ArgumentNullException(nameof(payrollCalculatorProvider));

    /// <summary>
    /// Get tenant calendar period
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="cultureName">The culture to use</param>
    /// <param name="calendarName">The calendar name</param>
    /// <param name="periodMoment">The period evaluation date</param>
    /// <param name="offset">The offset:<br />
    /// less than zero: past<br />
    /// zero: current<br />
    /// greater than zero: future<br /></param>
    /// <returns>The calendar period</returns>
    public virtual async Task<ActionResult<DatePeriod>> GetCalendarPeriodAsync(int tenantId, string cultureName,
        string calendarName, DateTime? periodMoment, int? offset)
    {
        try
        {
            // tenant
            var tenant = await ParentService.GetAsync(Runtime.DbContext, tenantId);
            if (tenant == null)
            {
                return BadRequest($"Unknown tenant with id {tenantId}");
            }

            // calendar
            Calendar calendar;
            if (string.IsNullOrWhiteSpace(calendarName))
            {
                // fallback 1: default calendar
                calendarName = tenant.Calendar;
            }
            if (!string.IsNullOrWhiteSpace(calendarName))
            {
                calendar = await Service.GetByNameAsync(Runtime.DbContext, tenantId, calendarName);
                if (calendar == null)
                {
                    return BadRequest($"Unknown calendar {calendarName}");
                }
            }
            else
            {
                // fallback 2: default calendar
                calendar = new();
            }

            // culture
            if (string.IsNullOrWhiteSpace(cultureName))
            {
                // tenant culture
                if (string.IsNullOrWhiteSpace(cultureName))
                {
                    cultureName = tenant.Culture;
                }
            }
            var cultureInfo = NewCultureInfo(cultureName);
            if (cultureInfo == null)
            {
                return BadRequest($"Unknown culture: {cultureName}");
            }

            // calculator
            var calculator = PayrollCalculatorProvider.CreateCalculator(
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

    public virtual async Task<ActionResult<DatePeriod>> GetCalendarCycleAsync(int tenantId,
        string cultureName, string calendarName, DateTime? cycleMoment, int? offset)
    {
        try
        {
            // tenant
            var tenant = await ParentService.GetAsync(Runtime.DbContext, tenantId);
            if (tenant == null)
            {
                return BadRequest($"Unknown tenant with id {tenantId}");
            }

            // calendar
            Calendar calendar;
            if (string.IsNullOrWhiteSpace(calendarName))
            {
                // fallback 1: default calendar
                calendarName = tenant.Calendar;
            }
            if (!string.IsNullOrWhiteSpace(calendarName))
            {
                calendar = await Service.GetByNameAsync(Runtime.DbContext, tenantId, calendarName);
                if (calendar == null)
                {
                    return BadRequest($"Unknown calendar {calendarName}");
                }
            }
            else
            {
                // fallback 2: default calendar
                calendar = new();
            }

            // culture
            if (string.IsNullOrWhiteSpace(cultureName))
            {
                // tenant culture
                if (string.IsNullOrWhiteSpace(cultureName))
                {
                    cultureName = tenant.Culture;
                }
            }
            var cultureInfo = NewCultureInfo(cultureName);
            if (cultureInfo == null)
            {
                return BadRequest($"Unknown culture: {cultureName}");
            }

            // calculator
            var calculator = PayrollCalculatorProvider.CreateCalculator(
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

    public virtual async Task<ActionResult<decimal?>> CalculateCalendarValueAsync(int tenantId,
       decimal value, string cultureName, string calendarName,
       DateTime? evaluationDate, DateTime? evaluationPeriodDate)
    {
        try
        {
            // calendar
            if (string.IsNullOrWhiteSpace(calendarName))
            {
                return BadRequest("Missing calendar name");
            }
            var calendar = await Service.GetByNameAsync(Runtime.DbContext, tenantId, calendarName);
            if (calendar == null)
            {
                return BadRequest($"Unknown calendar {calendarName}");
            }

            // culture
            if (string.IsNullOrWhiteSpace(cultureName))
            {
                var tenant = await ParentService.GetAsync(Runtime.DbContext, tenantId);
                // tenant culture
                if (string.IsNullOrWhiteSpace(cultureName))
                {
                    cultureName = tenant.Culture;
                }
            }
            var cultureInfo = NewCultureInfo(cultureName);
            if (cultureInfo == null)
            {
                return BadRequest($"Unknown culture: {cultureName}");
            }

            // calculator
            var calculator = PayrollCalculatorProvider.CreateCalculator(
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