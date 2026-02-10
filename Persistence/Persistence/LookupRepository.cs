using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class LookupRepository(IRegulationRepository regulationRepository,
    ILookupAuditRepository auditRepository, bool auditEnabled) :
    LookupRepositoryBase<Lookup>(regulationRepository, auditRepository, auditEnabled),
    ILookupRepository;