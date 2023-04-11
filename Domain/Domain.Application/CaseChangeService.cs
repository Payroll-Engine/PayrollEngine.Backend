using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;

namespace PayrollEngine.Domain.Application;

public abstract class CaseChangeService<TRepo, TDomain> : ChildApplicationService<TRepo, TDomain>, ICaseChangeService<TRepo, TDomain>
    where TRepo : class, ICaseChangeRepository<TDomain>
    where TDomain : CaseChange, new()
{
    public IWebhookDispatchService WebhookDispatcher { get; }

    protected CaseChangeService(IWebhookDispatchService webhookDispatcher, TRepo caseChangeRepository) :
        base(caseChangeRepository)
    {
        WebhookDispatcher = webhookDispatcher ?? throw new ArgumentNullException(nameof(webhookDispatcher));
    }

    public virtual async Task<IEnumerable<TDomain>> QueryAsync(int tenantId, int parentId, Query query = null) =>
        await Repository.QueryAsync(tenantId, parentId, query);

    public virtual async Task<long> QueryCountAsync(int tenantId, int parentId, Query query = null) =>
        await Repository.QueryCountAsync(tenantId, parentId, query);

    public virtual async Task<IEnumerable<CaseChangeCaseValue>> QueryValuesAsync(int tenantId, int parentId, Query query = null) =>
        await Repository.QueryValuesAsync(tenantId, parentId, query);

    public virtual async Task<long> QueryValuesCountAsync(int tenantId, int parentId, Query query = null) =>
        await Repository.QueryValuesCountAsync(tenantId, parentId, query);

    public virtual async Task<TDomain> AddCaseChangeAsync(int tenantId, int userId, int payrollId, int parentId, TDomain change)
    {
        change = await Repository.AddCaseChangeAsync(tenantId, payrollId, parentId, change);

        // webhook
        if (change != null)
        {
            var json = DefaultJsonSerializer.Serialize(change);
            await WebhookDispatcher.SendMessageAsync(tenantId,
                new()
                {
                    Action = WebhookAction.CaseChangeAdded,
                    RequestMessage = json
                },
                userId: userId);
        }

        return change;
    }
}