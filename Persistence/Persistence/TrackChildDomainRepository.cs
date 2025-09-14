using System;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Persistence;

public abstract class TrackChildDomainRepository<TDomain, TAudit>(string tableName, string parentFieldName,
        IAuditChildDomainRepository<TAudit> auditRepository, bool auditDisabled)
    : ChildDomainRepository<TDomain>(tableName, parentFieldName),
        ITrackChildDomainRepository<TDomain, TAudit>
    where TDomain : TrackDomainObject<TAudit>, new()
    where TAudit : AuditDomainObject
{
    private IAuditChildDomainRepository<TAudit> AuditRepository { get; } = auditRepository ?? throw new ArgumentNullException(nameof(auditRepository));
    private bool AuditDisabled { get; } = auditDisabled;

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