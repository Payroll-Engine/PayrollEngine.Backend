using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class LookupRepository(IRegulationRepository regulationRepository,
    ILookupAuditRepository auditRepository, bool auditDisabled) :
    LookupRepositoryBase<Lookup>(regulationRepository, auditRepository, auditDisabled),
    ILookupRepository;