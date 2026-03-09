using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;

namespace PayrollEngine.Domain.Application;

public abstract class CaseChangeService<TRepo, TDomain>(IWebhookDispatchService webhookDispatcher,
        TRepo caseChangeRepository)
    : ChildApplicationService<TRepo, TDomain>(caseChangeRepository), ICaseChangeService<TRepo, TDomain>
    where TRepo : class, ICaseChangeRepository<TDomain>
    where TDomain : CaseChange, new()
{
    private IWebhookDispatchService WebhookDispatcher { get; } = webhookDispatcher ?? throw new ArgumentNullException(nameof(webhookDispatcher));

    public virtual async Task<IEnumerable<TDomain>> QueryAsync(IDbContext context, int tenantId, int parentId, Query query = null) =>
        await Repository.QueryAsync(context, tenantId, parentId, query);

    public virtual async Task<long> QueryCountAsync(IDbContext context, int tenantId, int parentId, Query query = null) =>
        await Repository.QueryCountAsync(context, tenantId, parentId, query);

    public virtual async Task<IEnumerable<CaseChangeCaseValue>> QueryValuesAsync(IDbContext context, int tenantId, int parentId, Query query = null) =>
        await Repository.QueryValuesAsync(context, tenantId, parentId, query);

    public virtual async Task<long> QueryValuesCountAsync(IDbContext context, int tenantId, int parentId, Query query = null) =>
        await Repository.QueryValuesCountAsync(context, tenantId, parentId, query);

    public virtual async Task<TDomain> AddCaseChangeAsync(IDbContext context, int tenantId, int userId, int payrollId, int parentId, TDomain change)
    {
        change = await Repository.AddCaseChangeAsync(context, tenantId, payrollId, parentId, change);

        // no webhook
        if (change == null || !await webhookDispatcher.HasWebhooksAsync(context, tenantId))
        {
            return change;
        }

        // webhook
        var json = DefaultJsonSerializer.Serialize(change);
        await WebhookDispatcher.SendMessageAsync(context, tenantId,
            new()
            {
                Action = WebhookAction.CaseChangeAdded,
                RequestMessage = json
            },
            userId: userId);

        return change;
    }

    public virtual async Task<List<TDomain>> AddCaseChangesAsync(IDbContext context, int tenantId, int userId, int payrollId,
        IEnumerable<(int ParentId, TDomain Change)> changes)
    {
        var results = await Repository.AddCaseChangesAsync(context, tenantId, payrollId, changes);

        // batch webhook
        if (results.Count > 0 && await webhookDispatcher.HasWebhooksAsync(context, tenantId))
        {
            foreach (var change in results)
            {
                var json = DefaultJsonSerializer.Serialize(change);
                await WebhookDispatcher.SendMessageAsync(context, tenantId,
                    new()
                    {
                        Action = WebhookAction.CaseChangeAdded,
                        RequestMessage = json
                    },
                    userId: userId);
            }
        }

        return results;
    }
}