using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class CaseRelationService(ICaseRelationRepository repository) :
    ScriptTrackChildApplicationService<ICaseRelationRepository, CaseRelation, CaseRelationAudit>(repository),
    ICaseRelationService;