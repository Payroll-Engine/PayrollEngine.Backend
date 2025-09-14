using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class CaseChangeRepositorySettings
{
    public ITenantRepository TenantRepository { get; init; }
    public IDivisionRepository DivisionRepository { get; init; }
    public IEmployeeRepository EmployeeRepository { get; init; }
    public IPayrollRepository PayrollRepository { get; init; }
    public ICaseRepository CaseRepository { get; init; }
    public ICaseFieldRepository CaseFieldRepository { get; init; }
    public ICaseValueRepository CaseValueRepository { get; init; }
    public ICaseValueSetupRepository CaseValueSetupRepository { get; init; }
    public ICaseValueChangeRepository CaseValueChangeRepository { get; init; }
}