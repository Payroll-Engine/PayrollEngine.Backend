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
    public IUserRepository UserRepository { get; set; }
    public IDivisionRepository DivisionRepository { get; set; }
    public IEmployeeRepository EmployeeRepository { get; set; }
    public ICaseRepository CaseRepository { get; set; }
    public IGlobalCaseValueRepository GlobalCaseValueRepository { get; set; }
    public INationalCaseValueRepository NationalCaseValueRepository { get; set; }
    public ICompanyCaseValueRepository CompanyCaseValueRepository { get; set; }
    public IEmployeeCaseValueRepository EmployeeCaseValueRepository { get; set; }
    public IPayrunRepository PayrunRepository { get; set; }
    public IPayrunJobRepository PayrunJobRepository { get; set; }
    public ICollectorRepository CollectorRepository { get; set; }
    public IWageTypeRepository WageTypeRepository { get; set; }
    public ILookupSetRepository RegulationLookupSetRepository { get; set; }
    public IRegulationRepository RegulationRepository { get; set; }
    public IRegulationShareRepository RegulationShareRepository { get; set; }
    public IPayrollRepository PayrollRepository { get; set; }
    public IPayrollResultRepository PayrollResultRepository { get; set; }
    public IPayrollConsolidatedResultRepository PayrollConsolidatedResultRepository { get; set; }
    public IPayrollResultSetRepository PayrollResultSetRepository { get; set; }

    /// <summary>Function log timeout</summary>
    public TimeSpan FunctionLogTimeout { get; set; }

    // services
    public IWebhookDispatchService WebhookDispatchService { get; set; }
}