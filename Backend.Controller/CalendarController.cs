using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using System.ComponentModel.DataAnnotations;
using System;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Calendars")]
[Route("api/tenants/{tenantId}/calendars")]
public class CalendarController : Api.Controller.CalendarController
{
    /// <inheritdoc/>
    public CalendarController(ITenantService tenantService, ICalendarService calendarService,
        IPayrollCalculatorProvider payrollCalculatorProvider, IControllerRuntime runtime) :
        base(tenantService, calendarService, payrollCalculatorProvider, runtime)
    {
    }

    /// <summary>
    /// Query calendars
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The tenant calendars</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryCalendars")]
    public async Task<ActionResult> QueryCalendarsAsync(int tenantId, [FromQuery] Query query)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await QueryItemsAsync(tenantId, query);
    }

    /// <summary>
    /// Get a calendar
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="calendarId">The id of the calendar</param>
    /// <returns></returns>
    [HttpGet("{calendarId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetCalendar")]
    public async Task<ActionResult<ApiObject.Calendar>> GetCalendarAsync(int tenantId, int calendarId)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await GetAsync(tenantId, calendarId);
    }

    /// <summary>
    /// Add a new calendar
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="calendar">The calendar to add</param>
    /// <returns>The newly created calendar</returns>
    [HttpPost]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("CreateCalendar")]
    public async Task<ActionResult<ApiObject.Calendar>> CreateCalendarAsync(int tenantId, ApiObject.Calendar calendar)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }

        // calendar check
        if (!MapApiToDomain(calendar).ValidTimeUnits())
        {
            return BadRequest($"Calendar {calendar.Name} with invalid time units: Cycle={calendar.CycleTimeUnit}, Period={calendar.PeriodTimeUnit}");
        }

        return await CreateAsync(tenantId, calendar);
    }

    /// <summary>
    /// Update a calendar
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="calendar">The calendar with updated values</param>
    /// <returns>The modified calendar</returns>
    [HttpPut("{calendarId}")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("UpdateCalendar")]
    public async Task<ActionResult<ApiObject.Calendar>> UpdateCalendarAsync(int tenantId, ApiObject.Calendar calendar)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }

        // calendar check
        if (!MapApiToDomain(calendar).ValidTimeUnits())
        {
            return BadRequest($"Calendar {calendar.Name} with invalid time units: Cycle={calendar.CycleTimeUnit}, Period={calendar.PeriodTimeUnit}");
        }

        return await UpdateAsync(tenantId, calendar);
    }

    /// <summary>
    /// Delete a calendar
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="calendarId">The id of the calendar</param>
    /// <returns></returns>
    [HttpDelete("{calendarId}")]
    [ApiOperationId("DeleteCalendar")]
    public async Task<IActionResult> DeleteCalendarAsync(int tenantId, int calendarId)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await DeleteAsync(tenantId, calendarId);
    }

    /// <summary>
    /// Get calendar period
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="cultureName">The culture to use (default: tenant culture)</param>
    /// <param name="calendarName">The calendar configuration (default: tenant calendar)</param>
    /// <param name="periodMoment">The moment within the payrun period (default: now)</param>
    /// <param name="offset">The offset:<br />
    /// less than zero: past<br />
    /// zero: current (default)<br />
    /// greater than zero: future<br /></param>
    /// <returns>The calendar period</returns>
    [HttpGet("periods")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("GetCalendarPeriod")]
    public override async Task<ActionResult<DatePeriod>> GetCalendarPeriodAsync(int tenantId,
        [FromQuery] string cultureName, [FromQuery] string calendarName,
        [FromQuery] DateTime? periodMoment, [FromQuery] int? offset)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await base.GetCalendarPeriodAsync(tenantId, cultureName, calendarName, periodMoment, offset);
    }

    /// <summary>
    /// Get calendar cycle
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="cultureName">The culture to use (default: tenant culture)</param>
    /// <param name="calendarName">The calendar configuration (default: tenant calendar)</param>
    /// <param name="cycleMoment">The moment within the payrun cycle (default: now)</param>
    /// <param name="offset">The offset:<br />
    /// less than zero: past<br />
    /// zero: current (default)<br />
    /// greater than zero: future<br /></param>
    /// <returns>The calendar cycle</returns>
    [HttpGet("cycles")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("GetCalendarCycle")]
    public override async Task<ActionResult<DatePeriod>> GetCalendarCycleAsync(int tenantId,
        [FromQuery] string cultureName, [FromQuery] string calendarName,
        [FromQuery] DateTime? cycleMoment,
        [FromQuery] int? offset)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await base.GetCalendarCycleAsync(tenantId, cultureName, calendarName, cycleMoment, offset);
    }

    /// <summary>
    /// Calculate calendar value
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="value">The value to calculate</param>
    /// <param name="cultureName">The culture to use (default: tenant culture)</param>
    /// <param name="calendarName">The calendar configuration (default: tenant calendar)</param>
    /// <param name="evaluationDate">The evaluation period date (default: all)</param>
    /// <param name="evaluationPeriodDate">The date within the evaluation period (default: evaluation date)</param>
    /// <returns>The calendar value</returns>
    [HttpGet("values")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("CalculateCalendarValue")]
    [QueryIgnore]
    public override async Task<ActionResult<decimal?>> CalculateCalendarValueAsync(int tenantId,
        [FromQuery][Required] decimal value, [FromQuery] string cultureName, [FromQuery] string calendarName,
        [FromQuery] DateTime? evaluationDate, [FromQuery] DateTime? evaluationPeriodDate
       )
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await base.CalculateCalendarValueAsync(tenantId, value, cultureName, calendarName, evaluationDate, evaluationPeriodDate);
    }
}