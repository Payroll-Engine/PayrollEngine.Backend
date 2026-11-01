using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Domain.Application;

/// <summary>
/// Webhook service: protects the webhook secret before it is stored
/// </summary>
public class WebhookService(IWebhookRepository repository, IWebhookSecretProtector secretProtector) :
    ChildApplicationService<IWebhookRepository, Webhook>(repository), IWebhookService
{
    private IWebhookSecretProtector SecretProtector { get; } =
        secretProtector ?? throw new ArgumentNullException(nameof(secretProtector));

    /// <inheritdoc />
    public override async Task<Webhook> CreateAsync(IDbContext context, int parentId, Webhook item) =>
        await base.CreateAsync(context, parentId, ProtectSecret(item));

    /// <inheritdoc />
    public override async Task CreateBulkAsync(IDbContext context, int parentId, IEnumerable<Webhook> items) =>
        await base.CreateBulkAsync(context, parentId, items?.Select(ProtectSecret).ToList());

    /// <inheritdoc />
    public override async Task<IEnumerable<Webhook>> CreateAsync(IDbContext context, int parentId, IEnumerable<Webhook> items) =>
        await base.CreateAsync(context, parentId, items?.Select(ProtectSecret).ToList());

    /// <inheritdoc />
    public override async Task<Webhook> UpdateAsync(IDbContext context, int parentId, Webhook item) =>
        await base.UpdateAsync(context, parentId, ProtectSecret(item));

    // null (keep) and empty (remove) pass through, any other value is protected
    private Webhook ProtectSecret(Webhook webhook)
    {
        if (!string.IsNullOrEmpty(webhook?.Secret))
        {
            webhook.Secret = SecretProtector.Protect(webhook.Secret);
        }
        return webhook;
    }
}
