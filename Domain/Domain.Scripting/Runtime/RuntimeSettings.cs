﻿using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Scripting.Runtime;

public class RuntimeSettings
{
    /// <summary>The database context</summary>
    public IDbContext DbContext { get; init; }

    /// <summary>The function host</summary>
    public IFunctionHost FunctionHost { get; init; }

    /// <summary>The tenant</summary>
    public Tenant Tenant { get; init; }

    /// <summary>The user</summary>
    public User User { get; init; }

    /// <summary>The user culture</summary>
    public string UserCulture { get; init; }

    /// <summary>The division repository</summary>
    public IDivisionRepository DivisionRepository { get; init; }

    /// <summary>The employee repository</summary>
    public IEmployeeRepository EmployeeRepository { get; init; }

    /// <summary>The calendar repository</summary>
    public ICalendarRepository CalendarRepository { get; init; }

    /// <summary>The payroll calculator provider</summary>
    public IPayrollCalculatorProvider PayrollCalculatorProvider { get; init; }

    /// <summary>The webhook dispatch service</summary>
    public IWebhookDispatchService WebhookDispatchService { get; init; }
}