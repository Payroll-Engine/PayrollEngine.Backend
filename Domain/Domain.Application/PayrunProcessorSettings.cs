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
    public IPayrunRepository PayrunRepository { get; init; }
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

    // services
    public ICalendarRepository CalendarRepository { get; init; }
    public IPayrollCalculatorProvider PayrollCalculatorProvider { get; init; }
    public IWebhookDispatchService WebhookDispatchService { get; init; }
}