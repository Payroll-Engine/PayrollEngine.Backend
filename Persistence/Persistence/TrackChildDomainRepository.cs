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
        IAuditChildDomainRepository<TAudit> auditRepository, IDbContext context) :
        base(tableName, parentFieldName, context)
    {
        AuditRepository = auditRepository ?? throw new ArgumentNullException(nameof(auditRepository));
    }

    #region Audit

    public virtual async Task<TAudit> GetCurrentAuditAsync(int trackObjectId) =>
        await AuditRepository.GetCurrentAuditAsync(trackObjectId);

    public virtual TDomain NewFromAudit(TAudit audit)
    {
        var item = new TDomain();
        item.FromAuditObject(audit);
        return item;
    }

    protected virtual TAudit CreateAuditObject(TDomain item) =>
        item.ToAuditObject();

    protected override async Task OnCreatedAsync(int parentId, TDomain item)
    {
        // create audit record after a new track item has been created
        TAudit audit = CreateAuditObject(item);
        await AuditRepository.CreateAsync(item.Id, audit);
    }

    protected override async Task OnUpdatedAsync(int parentId, TDomain item)
    {
        // create audit object after updating the tracked item
        TAudit audit = CreateAuditObject(item);
        await AuditRepository.CreateAsync(item.Id, audit);
    }

    protected override async Task<bool> OnDeletingAsync(int parentId, int itemId)
    {
        // remove all audit records before deleting the track
        var audits = await AuditRepository.QueryAsync(itemId);
        if (audits != null)
        {
            foreach (var audit in audits)
            {
                await AuditRepository.DeleteAsync(itemId, audit.Id);
            }
        }
        return true;
    }

    #endregion
}