using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Task = System.Threading.Tasks.Task;
using PayrollEngine.Domain.Model;
using PayrollEngine.Serialization;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

/// <summary>
/// Simple webhook dispatcher using IHttpClientFactory for proper DNS rotation and connection pooling.
/// The default webhook timeout is 5 seconds, configurable via named HttpClient registration.
/// </summary>
public class WebhookDispatchService(ITenantRepository tenantRepository, IUserRepository userRepository,
        IWebhookRepository webhookRepository, IWebhookMessageRepository messageRepository,
        IHttpClientFactory httpClientFactory)
    : IWebhookDispatchService
{
    /// <summary>
    /// Named HttpClient identifier for DI configuration
    /// </summary>
    public static readonly string HttpClientName = "WebhookDispatch";

    private ITenantRepository TenantRepository { get; } = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
    private IUserRepository UserRepository { get; } = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    private IWebhookRepository WebhookRepository { get; } = webhookRepository ?? throw new ArgumentNullException(nameof(webhookRepository));
    private IWebhookMessageRepository MessageRepository { get; } = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
    private IHttpClientFactory HttpClientFactory { get; } = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

    /// <inheritdoc />
    public async Task<bool> HasWebhooksAsync(IDbContext context, int tenantId)
    {
        var webhooks = await WebhookRepository.QueryAsync(context, tenantId, new()
        {
            Status = ObjectStatus.Active
        });
        return webhooks.Any();
    }

    /// <inheritdoc />
    public async Task<string> InvokeAsync(IDbContext context, int tenantId, WebhookDispatchMessage dispatchMessage, int? userId = null)
    {
        // tenant
        var tenant = await TenantRepository.GetAsync(context, tenantId);
        if (tenant == null)
        {
            throw new PayrollException($"Unknown tenant with id {tenantId}.");
        }

        // user
        User user = null;
        if (userId.HasValue)
        {
            user = await UserRepository.GetAsync(context, tenantId, userId.Value);
        }

        // single and active webhook matching the action
        var webhooks = await QueryWebhooks(context, tenantId, dispatchMessage.Action);
        if (webhooks.Count != 1)
        {
            Log.Warning($"missing single web hook for action {dispatchMessage.Action}");
            return null;
        }

        var webhook = webhooks.First();
        return await DispatchWebhook(context, tenant.Identifier, user?.Identifier, webhook, dispatchMessage);
    }

    /// <inheritdoc />
    public async Task SendMessageAsync(IDbContext context, int tenantId, WebhookDispatchMessage dispatchMessage, int? userId = null)
    {
        if (tenantId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tenantId));
        }
        if (dispatchMessage == null)
        {
            throw new ArgumentNullException(nameof(dispatchMessage));
        }

        // tenant
        var tenant = await TenantRepository.GetAsync(context, tenantId);
        if (tenant == null)
        {
            throw new PayrollException($"Unknown tenant with id {tenantId}.");
        }

        // webhooks
        var webhooks = await QueryWebhooks(context, tenantId, dispatchMessage.Action);
        if (!webhooks.Any())
        {
            // no matching webhook
            return;
        }

        // user
        User user = null;
        if (userId.HasValue)
        {
            user = await UserRepository.GetAsync(context, tenantId, userId.Value);
        }

        // send message to any webhook
        foreach (var webhook in webhooks)
        {
            await DispatchWebhook(context, tenant.Identifier, user?.Identifier, webhook, dispatchMessage);
        }
    }

    /// <summary>
    /// Dispatch a single webhook: build the runtime message, optionally persist tracking,
    /// POST to the receiver, and record the response.
    /// </summary>
    private async Task<string> DispatchWebhook(IDbContext context, string tenant, string user,
        Webhook webhook, WebhookDispatchMessage dispatchMessage)
    {
        if (dispatchMessage == null)
        {
            throw new ArgumentNullException(nameof(dispatchMessage));
        }

        Log.Debug($"preparing web hook {webhook.Name} (action={webhook.Action}");

        // webhook request
        var webhookMessage = new WebhookRuntimeMessage
        {
            Tenant = tenant,
            User = user,
            ReceiverAddress = webhook.ReceiverAddress,
            RequestDate = Date.Now,
            RequestMessage = dispatchMessage.RequestMessage,
            RequestOperation = dispatchMessage.RequestOperation,
            ActionName = Enum.GetName(typeof(WebhookAction), dispatchMessage.Action)
        };

        // track message start
        if (dispatchMessage.TrackMessage)
        {
            // create webhook message
            await MessageRepository.CreateAsync(context, webhook.Id, webhookMessage);
        }

        // post - each call gets a fresh HttpClient from the factory,
        // ensuring proper DNS rotation and connection pooling
        Log.Debug($"Sending web hook {webhook.Name} to {webhook.ReceiverAddress}");
        try
        {
            var httpClient = HttpClientFactory.CreateClient(HttpClientName);
            var jsonMessage = DefaultJsonSerializer.Serialize(webhookMessage);
            var message = DefaultJsonSerializer.SerializeJson(jsonMessage);
            var response = await httpClient.PostAsync(webhook.ReceiverAddress, message);
            webhookMessage.ResponseStatus = (int)response.StatusCode;
            if (response.IsSuccessStatusCode)
            {
                webhookMessage.ResponseMessage = await response.Content.ReadAsStringAsync();
            }
        }
        catch (Exception exception)
        {
            var errorMessage = exception.GetBaseMessage();
            Log.Error(exception, errorMessage);
            webhookMessage.ResponseMessage = errorMessage;
        }

        // webhook response
        // ignore failed status to ensure execution of remaining webhooks
        webhookMessage.ResponseDate = Date.Now;

        // track message start
        if (dispatchMessage.TrackMessage)
        {
            // update webhook message
            await MessageRepository.UpdateAsync(context, webhook.Id, webhookMessage);
        }

        Log.Debug($"completed web hook {webhook.Name} with response status {webhookMessage.ResponseStatus}");

        return webhookMessage.ResponseMessage;
    }

    private async Task<List<Webhook>> QueryWebhooks(IDbContext context, int tenantId, WebhookAction action) =>
        (await WebhookRepository.QueryAsync(context, tenantId, new()
        {
            Status = ObjectStatus.Active,
            Filter = $"{nameof(Webhook.Action)} eq '{action}'"
        })).ToList();
}
