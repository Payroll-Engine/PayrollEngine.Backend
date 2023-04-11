using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class CaseChangeRepositorySettings
{
    public IPayrollRepository PayrollRepository { get; set; }
    public ICaseRepository CaseRepository { get; set; }
    public ICaseFieldRepository CaseFieldRepository { get; set; }
    public ICaseValueRepository CaseValueRepository { get; set; }
    public ICaseValueSetupRepository CaseValueSetupRepository { get; set; }
    public ICaseValueChangeRepository CaseValueChangeRepository { get; set; }
}