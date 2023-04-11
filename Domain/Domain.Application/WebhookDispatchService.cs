using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Domain.Application;

/// <summary>
/// Simple webhook dispatcher without support of any authentication and security requirements
/// The default webhook timeout is 5 seconds
/// </summary>
public class WebhookDispatchService : IWebhookDispatchService
{
    private static HttpClient HttpClient { get; }
    // TODO webhook timeout setup
    public static TimeSpan Timeout
    {
        get => HttpClient.Timeout;
        set => HttpClient.Timeout = value;
    }

    public ITenantRepository TenantRepository { get; }
    public IUserRepository UserRepository { get; }
    public IWebhookRepository WebhookRepository { get; }
    public IWebhookMessageRepository MessageRepository { get; }

    /// <summary>
    /// Only one http client
    /// prevent sockets-overflow exception, by creating the http client for any request.
    /// See https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient.-ctor?f1url=https%3A%2F%2Fmsdn.microsoft.com%2Fquery%2Fdev16.query%3FappId%3DDev16IDEF1%26l%3DEN-US%26k%3Dk(System.Net.Http.HttpClient.%2523ctor);k(DevLang-csharp)
    /// </summary>
    static WebhookDispatchService()
    {
        HttpClient = new();
    }

    public WebhookDispatchService(ITenantRepository tenantRepository, IUserRepository userRepository,
        IWebhookRepository webhookRepository, IWebhookMessageRepository messageRepository)
    {
        TenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
        UserRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        WebhookRepository = webhookRepository ?? throw new ArgumentNullException(nameof(webhookRepository));
        MessageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
    }

    /// <inheritdoc />
    public virtual async Task<string> InvokeAsync(int tenantId, WebhookDispatchMessage dispatchMessage, int? userId = null)
    {
        // tenant
        var tenant = await TenantRepository.GetAsync(tenantId);
        if (tenant == null)
        {
            throw new PayrollException($"Unknown tenant with id {tenantId}");
        }

        // user
        User user = null;
        if (userId.HasValue)
        {
            user = await UserRepository.GetAsync(tenantId, userId.Value);
        }

        // single and active webhook matching the action
        var webhooks = await QueryWebhooks(tenantId, dispatchMessage.Action);
        if (webhooks.Count != 1)
        {
            Log.Warning($"missing single web hook for action {dispatchMessage.Action}");
            return default;
        }

        var webhook = webhooks.First();
        return await DispatchWebhook(tenant.Identifier, user?.Identifier, webhook, dispatchMessage);
    }

    /// <inheritdoc />
    public virtual async Task SendMessageAsync(int tenantId, WebhookDispatchMessage dispatchMessage, int? userId = null)
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
        var tenant = await TenantRepository.GetAsync(tenantId);
        if (tenant == null)
        {
            throw new PayrollException($"Unknown tenant with id {tenantId}");
        }

        // webhooks
        var webhooks = await QueryWebhooks(tenantId, dispatchMessage.Action);
        if (!webhooks.Any())
        {
            // no matching webhook
            return;
        }

        // user
        User user = null;
        if (userId.HasValue)
        {
            user = await UserRepository.GetAsync(tenantId, userId.Value);
        }

        // send message to any webhook
        foreach (var webhook in webhooks)
        {
            await DispatchWebhook(tenant.Identifier, user?.Identifier, webhook, dispatchMessage);
        }
    }

    private async Task<string> DispatchWebhook(string tenant, string user, Webhook webhook, WebhookDispatchMessage dispatchMessage)
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
            await MessageRepository.CreateAsync(webhook.Id, webhookMessage);
        }

        // post
        Log.Debug($"Sending web hook {webhook.Name} to {webhook.ReceiverAddress}");
        try
        {
            var jsonMessage = DefaultJsonSerializer.Serialize(webhookMessage);
            var message = DefaultJsonSerializer.SerializeJson(jsonMessage);
            var response = await HttpClient.PostAsync(webhook.ReceiverAddress, message);
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
            await MessageRepository.UpdateAsync(webhook.Id, webhookMessage);
        }

        Log.Debug($"completed web hook {webhook.Name} with response status {webhookMessage.ResponseStatus}");

        return webhookMessage.ResponseMessage;
    }

    private async Task<List<Webhook>> QueryWebhooks(int tenantId, WebhookAction action) =>
        (await WebhookRepository.QueryAsync(tenantId, new()
        {
            Status = ObjectStatus.Active,
            Filter = $"{nameof(Webhook.Action)} eq '{action}'"
        })).ToList();
}