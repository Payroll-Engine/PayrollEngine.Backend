using System;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

/// <summary>
/// Setting for the <see cref="PayrunProcessor"/>
/// </summary>
public class PayrunProcessorSettings : FunctionToolSettings
{
    // repositories
    public IUserRepository UserRepository { get; init; }
    public IDivisionRepository DivisionRepository { get; init; }
    public IEmployeeRepository EmployeeRepository { get; init; }
    public IGlobalCaseValueRepository GlobalCaseValueRepository { get; init; }
    public INationalCaseValueRepository NationalCaseValueRepository { get; init; }
    public ICompanyCaseValueRepository CompanyCaseValueRepository { get; init; }
    public IEmployeeCaseValueRepository EmployeeCaseValueRepository { get; init; }
    public IPayrunJobRepository PayrunJobRepository { get; init; }
    public ILookupSetRepository RegulationLookupSetRepository { get; init; }
    public IRegulationRepository RegulationRepository { get; init; }
    public IRegulationShareRepository RegulationShareRepository { get; init; }
    public IPayrollRepository PayrollRepository { get; init; }
    public IPayrollResultRepository PayrollResultRepository { get; init; }
    public IPayrollConsolidatedResultRepository PayrollConsolidatedResultRepository { get; init; }
    public IPayrollResultSetRepository PayrollResultSetRepository { get; init; }

    /// <summary>Function log timeout</summary>
    public TimeSpan FunctionLogTimeout { get; init; }

    /// <summary>Log employee processing timing (started, per-employee duration, completed summary). Default: false</summary>
    public bool LogEmployeeTiming { get; init; }

    /// <summary>
    /// Maximum degree of parallelism for employee processing.
    /// 0 = sequential (default, no parallelism),
    /// -1 = automatic (based on Environment.ProcessorCount),
    /// 1 to N = explicit maximum thread count.
    /// </summary>
    public int MaxParallelEmployees { get; init; }

    /// <summary>
    /// Maximum number of retro payrun periods per employee.
    /// 0 = unlimited (no guard). Acts as a defense-in-depth limit
    /// independent of <see cref="PayrollEngine.RetroTimeType"/>.
    /// </summary>
    public int MaxRetroPayrunPeriods { get; init; }

    /// <summary>
    /// Processing mode: <see cref="PayrunProcessorMode.Persist"/> (default) writes results
    /// to the database, <see cref="PayrunProcessorMode.Preview"/> collects results in-memory
    /// without any DB writes for job or results.
    /// </summary>
    public PayrunProcessorMode Mode { get; init; } = PayrunProcessorMode.Persist;

    // services
    public ICalendarRepository CalendarRepository { get; init; }
    public IPayrollCalculatorProvider PayrollCalculatorProvider { get; init; }
    public IWebhookDispatchService WebhookDispatchService { get; init; }
}