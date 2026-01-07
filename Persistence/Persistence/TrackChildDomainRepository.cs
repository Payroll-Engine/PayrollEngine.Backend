using System;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public abstract class TrackChildDomainRepository<TDomain, TAudit>(IRegulationRepository regulationRepository,
    string tableName, string parentFieldName, IAuditChildDomainRepository<TAudit> auditRepository, bool auditDisabled)
    : ChildDomainRepository<TDomain>(tableName, parentFieldName),
        ITrackChildDomainRepository<TDomain, TAudit>
    where TDomain : TrackDomainObject<TAudit>, INamespaceObject, new()
    where TAudit : AuditDomainObject
{
    protected IRegulationRepository RegulationRepository { get; } = regulationRepository ?? throw new ArgumentNullException(nameof(regulationRepository));
    private IAuditChildDomainRepository<TAudit> AuditRepository { get; } = auditRepository ?? throw new ArgumentNullException(nameof(auditRepository));
    private bool AuditDisabled { get; } = auditDisabled;

    #region Namespace

    /// <summary>
    /// Get the regulation namespace
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="regulationId">Regulation id</param>
    /// <returns></returns>
    protected async Task<string> GetRegulationNamespaceAsync(IDbContext context, int regulationId)
    {
        var tenantId = await RegulationRepository.GetParentIdAsync(context, regulationId);
        if (tenantId == null || tenantId <= 0)
        {
            return null;
        }
        return await GetRegulationNamespaceAsync(context, tenantId.Value, regulationId);
    }

    /// <summary>
    /// Get the regulation namespace
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="tenantId">Tenant id</param>
    /// <param name="regulationId">Regulation id</param>
    /// <returns></returns>
    private async Task<string> GetRegulationNamespaceAsync(IDbContext context, int tenantId, int regulationId)
    {
        var query = new Query
        {
            Status = ObjectStatus.Active,
            Filter = $"{nameof(Regulation.Id)} eq {regulationId}",
            Select = $"{nameof(Regulation.Namespace)}"
        };

        var result = await RegulationRepository.QueryAsync(context, tenantId, query);
        var @namespace = result.FirstOrDefault()?.Namespace;
        return @namespace;
    }

    /// <summary>
    /// Apply namespace to item
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="regulationId">Regulation id</param>
    /// <param name="item">Item to apply the namespace</param>
    /// <returns></returns>
    protected async Task ApplyNamespaceAsync(IDbContext context, int regulationId, TDomain item)
    {
        var @namespace = await GetRegulationNamespaceAsync(context, regulationId);
        if (!string.IsNullOrWhiteSpace(@namespace))
        {
            item.ApplyNamespace(@namespace.EnsureEnd("."));
        }
    }

    #endregion

    #region Audit

    public virtual async Task<TAudit> GetCurrentAuditAsync(IDbContext context, int trackObjectId) =>
        await AuditRepository.GetCurrentAuditAsync(context, trackObjectId);

    public virtual TDomain NewFromAudit(TAudit audit)
    {
        var item = new TDomain();
        item.FromAuditObject(audit);
        return item;
    }

    private static TAudit CreateAuditObject(TDomain item) =>
        item.ToAuditObject();

    protected override async Task OnCreatedAsync(IDbContext context, int parentId, TDomain item)
    {
        if (AuditDisabled)
        {
            return;
        }

        // create audit record after a new track item has been created
        var audit = CreateAuditObject(item);
        await AuditRepository.CreateAsync(context, item.Id, audit);
    }

    protected override async Task OnUpdatedAsync(IDbContext context, int parentId, TDomain item)
    {
        if (AuditDisabled)
        {
            return;
        }

        // create audit object after updating the tracked item
        var audit = CreateAuditObject(item);
        await AuditRepository.CreateAsync(context, item.Id, audit);
    }

    protected override async Task<bool> OnDeletingAsync(IDbContext context, int itemId)
    {
        var deleting = await base.OnDeletingAsync(context, itemId);
        if (!deleting)
        {
            return false;
        }

        if (AuditDisabled)
        {
            return true;
        }

        // remove all audit records before deleting the track
        var audits = await AuditRepository.QueryAsync(context, itemId);
        if (audits != null)
        {
            foreach (var audit in audits)
            {
                await AuditRepository.DeleteAsync(context, itemId, audit.Id);
            }
        }
        return true;
    }

    #endregion
}