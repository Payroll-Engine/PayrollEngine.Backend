using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class LookupRepository(IRegulationRepository regulationRepository,
    ILookupAuditRepository auditRepository, bool auditEnabled) :
    LookupRepositoryBase<Lookup>(regulationRepository, auditRepository, auditEnabled),
    ILookupRepository
{
    /// <inheritdoc />
    /// <remarks>Applies the regulation namespace to the lookup name before persisting,
    /// consistent with the bulk-create path in LookupSetRepository CreateAsync.</remarks>
    public override async Task<Lookup> CreateAsync(IDbContext context, int regulationId, Lookup item)
    {
        await ApplyNamespaceAsync(context, regulationId, item);
        return await base.CreateAsync(context, regulationId, item);
    }
}