using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class CaseRelationService : ScriptTrackChildApplicationService<ICaseRelationRepository, CaseRelation, CaseRelationAudit>,
    ICaseRelationService
{
    public CaseRelationService(ICaseRelationRepository repository) :
        base(repository)
    {
    }
}