using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class LookupRepository(ILookupAuditRepository auditRepository) : LookupRepositoryBase<Lookup>(auditRepository),
    ILookupRepository;