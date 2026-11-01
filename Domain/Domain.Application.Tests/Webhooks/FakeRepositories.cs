using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Domain.Application.Tests.Webhooks;

// Hand-written fakes for the repositories used by the webhook dispatch service.
// Members which are not needed by the tests throw, so unexpected usage is detected.

/// <summary>Fake child repository base: all members throw unless overridden</summary>
internal abstract class FakeChildRepository<T> : IChildDomainRepository<T>
    where T : IDomainObject
{
    public string TypeName => typeof(T).Name;

    public virtual Task<bool> ExistsAsync(IDbContext context, int id) => throw new NotImplementedException();
    public virtual Task<bool> ExistsAsync(IDbContext context, string fieldName, object value) => throw new NotImplementedException();
    public virtual Task<string> GetAttributeAsync(IDbContext context, int id, string attributeName) => throw new NotImplementedException();
    public virtual Task<bool> ExistsAttributeAsync(IDbContext context, int id, string attributeName) => throw new NotImplementedException();
    public virtual Task<string> SetAttributeAsync(IDbContext context, int id, string attributeName, string value) => throw new NotImplementedException();
    public virtual Task<bool?> DeleteAttributeAsync(IDbContext context, int id, string attributeName) => throw new NotImplementedException();

    public virtual Task<bool> ExistsAsync(IDbContext context, int parentId, int id) => throw new NotImplementedException();
    public virtual Task<IEnumerable<T>> QueryAsync(IDbContext context, int parentId, Query query = null) => throw new NotImplementedException();
    public virtual Task<long> QueryCountAsync(IDbContext context, int parentId, Query query = null) => throw new NotImplementedException();
    public virtual Task<int?> GetParentIdAsync(IDbContext context, int itemId) => throw new NotImplementedException();
    public virtual Task<T> GetAsync(IDbContext context, int parentId, int itemId) => throw new NotImplementedException();
    public virtual Task<T> CreateAsync(IDbContext context, int parentId, T item) => throw new NotImplementedException();
    public virtual Task<IEnumerable<T>> CreateAsync(IDbContext context, int parentId, IEnumerable<T> items) => throw new NotImplementedException();
    public virtual Task CreateBulkAsync(IDbContext context, int parentId, IEnumerable<T> items) => throw new NotImplementedException();
    public virtual Task<T> UpdateAsync(IDbContext context, int parentId, T item) => throw new NotImplementedException();
    public virtual Task<bool> DeleteAsync(IDbContext context, int parentId, int itemId) => throw new NotImplementedException();
}

/// <summary>Fake webhook repository: supports the status and action filter of the dispatch service</summary>
internal sealed class FakeWebhookRepository : FakeChildRepository<Webhook>, IWebhookRepository
{
    public List<Webhook> Items { get; } = [];

    public override Task<Webhook> CreateAsync(IDbContext context, int parentId, Webhook item) =>
        Task.FromResult(item);

    public override Task<Webhook> UpdateAsync(IDbContext context, int parentId, Webhook item) =>
        Task.FromResult(item);

    /// <summary>The number of queries</summary>
    public int QueryCount { get; private set; }

    public override Task<IEnumerable<Webhook>> QueryAsync(IDbContext context, int parentId, Query query = null)
    {
        QueryCount++;
        IEnumerable<Webhook> result = Items;
        if (query?.Status != null)
        {
            result = result.Where(x => x.Status == query.Status.Value);
        }
        // filter format: Action eq 'CaseFunctionRequest'
        if (!string.IsNullOrWhiteSpace(query?.Filter))
        {
            var match = Regex.Match(query.Filter, @"^Action eq '(?<action>\w+)'$");
            if (!match.Success)
            {
                throw new NotSupportedException($"Unsupported filter: {query.Filter}");
            }
            var action = Enum.Parse<WebhookAction>(match.Groups["action"].Value);
            result = result.Where(x => x.Action == action);
        }
        return Task.FromResult<IEnumerable<Webhook>>(result.ToList());
    }
}

/// <summary>Fake user repository</summary>
internal sealed class FakeUserRepository : FakeChildRepository<User>, IUserRepository
{
    public List<User> Items { get; } = [];

    /// <summary>The number of user loads</summary>
    public int GetCount { get; private set; }

    public override Task<User> GetAsync(IDbContext context, int parentId, int itemId)
    {
        GetCount++;
        return Task.FromResult(Items.FirstOrDefault(x => x.Id == itemId));
    }

    public Task<bool> ExistsAnyAsync(IDbContext context, int tenantId, string identifier) => throw new NotImplementedException();
    public Task<bool> TestPasswordAsync(IDbContext context, int tenantId, int userId, string password) => throw new NotImplementedException();
    public Task UpdatePasswordAsync(IDbContext context, int tenantId, int userId, PasswordChangeRequest changeRequest) => throw new NotImplementedException();
}

/// <summary>Fake webhook message repository: records the create and update calls</summary>
internal sealed class FakeWebhookMessageRepository : FakeChildRepository<WebhookMessage>, IWebhookMessageRepository
{
    /// <summary>A recorded call with the state of the message at the time of the call</summary>
    internal sealed record Call(int WebhookId, string ActionName, DateTime ResponseDate,
        int ResponseStatus, string ResponseMessage);

    public List<Call> Created { get; } = [];
    public List<Call> Updated { get; } = [];

    public override Task<WebhookMessage> CreateAsync(IDbContext context, int parentId, WebhookMessage item)
    {
        Created.Add(Snapshot(parentId, item));
        return Task.FromResult(item);
    }

    public override Task<WebhookMessage> UpdateAsync(IDbContext context, int parentId, WebhookMessage item)
    {
        Updated.Add(Snapshot(parentId, item));
        return Task.FromResult(item);
    }

    // the dispatch service mutates the same message object: keep the state at call time
    private static Call Snapshot(int webhookId, WebhookMessage item) =>
        new(webhookId, item.ActionName, item.ResponseDate, item.ResponseStatus, item.ResponseMessage);
}

/// <summary>Fake tenant repository</summary>
internal sealed class FakeTenantRepository : ITenantRepository
{
    public List<Tenant> Items { get; } = [];

    public string TypeName => nameof(Tenant);

    public Task<Tenant> GetAsync(IDbContext context, int id) =>
        Task.FromResult(Items.FirstOrDefault(x => x.Id == id));

    public Task<bool> ExistsAsync(IDbContext context, int id) => throw new NotImplementedException();
    public Task<bool> ExistsAsync(IDbContext context, string fieldName, object value) => throw new NotImplementedException();
    public Task<string> GetAttributeAsync(IDbContext context, int id, string attributeName) => throw new NotImplementedException();
    public Task<bool> ExistsAttributeAsync(IDbContext context, int id, string attributeName) => throw new NotImplementedException();
    public Task<string> SetAttributeAsync(IDbContext context, int id, string attributeName, string value) => throw new NotImplementedException();
    public Task<bool?> DeleteAttributeAsync(IDbContext context, int id, string attributeName) => throw new NotImplementedException();
    public Task<IEnumerable<Tenant>> QueryAsync(IDbContext context, Query query = null) => throw new NotImplementedException();
    public Task<long> QueryCountAsync(IDbContext context, Query query = null) => throw new NotImplementedException();
    public Task<Tenant> CreateAsync(IDbContext context, Tenant item) => throw new NotImplementedException();
    public Task<IEnumerable<Tenant>> CreateAsync(IDbContext context, IEnumerable<Tenant> items) => throw new NotImplementedException();
    public Task<Tenant> UpdateAsync(IDbContext context, Tenant obj) => throw new NotImplementedException();
    public Task<bool> DeleteAsync(IDbContext context, int id) => throw new NotImplementedException();
    public Task<bool> ExistsAsync(IDbContext context, string identifier) => throw new NotImplementedException();
    public Task UpdateStatisticsAsync(IDbContext context) => throw new NotImplementedException();
}
