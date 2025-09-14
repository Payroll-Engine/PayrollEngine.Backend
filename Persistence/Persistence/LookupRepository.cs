using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class LookupRepository(ILookupAuditRepository auditRepository, bool auditDisabled) :
    LookupRepositoryBase<Lookup>(auditRepository, auditDisabled),
    ILookupRepository;