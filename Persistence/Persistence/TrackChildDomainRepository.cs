using System;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Persistence;

public abstract class TrackChildDomainRepository<TDomain, TAudit> : ChildDomainRepository<TDomain>,
    ITrackChildDomainRepository<TDomain, TAudit>
    where TDomain : TrackDomainObject<TAudit>, new()
    where TAudit : AuditDomainObject
{
    public IAuditChildDomainRepository<TAudit> AuditRepository { get; }

    protected TrackChildDomainRepository(string tableName, string parentFieldName,
        IAuditChildDomainRepository<TAudit> auditRepository ) :
        base(tableName, parentFieldName)
    {
        AuditRepository = auditRepository ?? throw new ArgumentNullException(nameof(auditRepository));
    }

    #region Audit

    public virtual async Task<TAudit> GetCurrentAuditAsync(IDbContext context, int trackObjectId) =>
        await AuditRepository.GetCurrentAuditAsync(context, trackObjectId);

    public virtual TDomain NewFromAudit(TAudit audit)
    {
        var item = new TDomain();
        item.FromAuditObject(audit);
        return item;
    }

    protected virtual TAudit CreateAuditObject(TDomain item) =>
        item.ToAuditObject();

    protected override async Task OnCreatedAsync(IDbContext context, int parentId, TDomain item)
    {
        // create audit record after a new track item has been created
        TAudit audit = CreateAuditObject(item);
        await AuditRepository.CreateAsync(context, item.Id, audit);
    }

    protected override async Task OnUpdatedAsync(IDbContext context, int parentId, TDomain item)
    {
        // create audit object after updating the tracked item
        TAudit audit = CreateAuditObject(item);
        await AuditRepository.CreateAsync(context, item.Id, audit);
    }

    protected override async Task<bool> OnDeletingAsync(IDbContext context, int parentId, int itemId)
    {
        var deleting = await base.OnDeletingAsync(context, parentId, itemId);
        if (!deleting)
        {
            return false;
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