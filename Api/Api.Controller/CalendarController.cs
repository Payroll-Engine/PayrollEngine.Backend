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
        IDivisionService divisionService, IEmployeeService employeeService, IPayrunJobService payrunJobService,
        IPayrollCalculatorProvider payrollCalculatorProvider, IControllerRuntime runtime)
    : RepositoryChildObjectController<ITenantService, ICalendarService,
    ITenantRepository, ICalendarRepository,
    Tenant, Calendar, ApiObject.Calendar>(tenantService, calendarService, runtime, new CalendarMap())
{
    private IDivisionService DivisionService { get; } = divisionService ?? throw new ArgumentNullException(nameof(divisionService));
    private IEmployeeService EmployeeService { get; } = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
    private IPayrunJobService PayrunJobService { get; } = payrunJobService ?? throw new ArgumentNullException(nameof(payrunJobService));
    private IPayrollCalculatorProvider PayrollCalculatorProvider { get; } = payrollCalculatorProvider ?? throw new ArgumentNullException(nameof(payrollCalculatorProvider));

    /// <inheritdoc />
    protected override async Task<ActionResult<ApiObject.Calendar>> UpdateAsync(int tenantId, ApiObject.Calendar calendar)
    {
        if (calendar != null && calendar.Id > 0)
        {
            // the period anchor date defines the week and bi-week period sequence of existing payrun jobs,
            // the cycle assignment defines the cycle of existing payrun results
            var existing = await Service.GetAsync(Runtime.DbContext, tenantId, calendar.Id);
            if (existing != null)
            {
                var anchorChanged = existing.PeriodAnchorDate?.Date != calendar.PeriodAnchorDate?.Date;
                var assignmentChanged = (existing.CycleAssignment ?? CalendarCycleAssignment.PayDate) !=
                                        (calendar.CycleAssignment ?? CalendarCycleAssignment.PayDate);
                if ((anchorChanged || assignmentChanged) &&
                    await IsCalendarInUseAsync(tenantId, existing.Name))
                {
                    return BadRequest(anchorChanged
                        ? $"The period anchor date of calendar {existing.Name} cannot be changed, payrun jobs exist for this calendar"
                        : $"The cycle assignment of calendar {existing.Name} cannot be changed, payrun jobs exist for this calendar");
                }
            }
        }
        return await base.UpdateAsync(tenantId, calendar);
    }

    /// <summary>
    /// Test for payrun jobs calculated with the calendar: jobs of divisions using the calendar,
    /// or any job when an employee uses the calendar
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="calendarName">The calendar name</param>
    private async Task<bool> IsCalendarInUseAsync(int tenantId, string calendarName)
    {
        var tenant = await ParentService.GetAsync(Runtime.DbContext, tenantId);
        var divisions = await DivisionService.QueryAsync(Runtime.DbContext, tenantId);
        foreach (var division in divisions)
        {
            var divisionCalendar = string.IsNullOrWhiteSpace(division.Calendar) ? tenant?.Calendar : division.Calendar;
            if (string.Equals(divisionCalendar, calendarName) &&
                await PayrunJobService.QueryCountAsync(Runtime.DbContext, tenantId,
                    new() { Filter = $"{nameof(PayrunJob.DivisionId)} eq {division.Id}" }) > 0)
            {
                return true;
            }
        }

        var employeeCount = await EmployeeService.QueryCountAsync(Runtime.DbContext, tenantId,
            new() { Filter = $"{nameof(Employee.Calendar)} eq '{calendarName.Replace("'", "''")}'" });
        return employeeCount > 0 &&
               await PayrunJobService.QueryCountAsync(Runtime.DbContext, tenantId) > 0;
    }

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