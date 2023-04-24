using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class LookupRepository : LookupRepositoryBase<Lookup>, ILookupRepository
{
    public LookupRepository(ILookupAuditRepository auditRepository) :
        base(auditRepository)
    {
    }
}