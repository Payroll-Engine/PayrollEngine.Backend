using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application.Service;

public interface IRegulationService : IChildApplicationService<IRegulationRepository, Regulation>
{
}