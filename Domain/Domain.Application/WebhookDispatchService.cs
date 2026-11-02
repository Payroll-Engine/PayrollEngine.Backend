using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Concurrent;
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
        IHttpClientFactory httpClientFactory, IWebhookSecretProtector secretProtector,
        WebhookTracking tracking = WebhookTracking.Notification)
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
    private IWebhookSecretProtector SecretProtector { get; } = secretProtector ?? throw new ArgumentNullException(nameof(secretProtector));

    private WebhookTracking Tracking { get; } = tracking;

    // Webhook lookup of the scripting calls per tenant and action. The service is registered per scope:
    // a payrun job (own scope) loads the webhooks once instead of once per script call, and sees a
    // consistent snapshot. The task is cached so parallel employees share a single query.
    private readonly ConcurrentDictionary<(int TenantId, WebhookAction Action), Task<List<Webhook>>> invokeWebhooks = new();

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
    public async Task<string> InvokeAsync(IDbContext context, Tenant tenant, User user,
        WebhookDispatchMessage dispatchMessage)
    {
        ArgumentNullException.ThrowIfNull(tenant);
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(dispatchMessage);

        // single and active webhook matching the action
        var webhooks = await QueryInvokeWebhooks(context, tenant.Id, dispatchMessage.Action);
        if (webhooks.Count != 1)
        {
            Log.Warning($"missing single web hook for action {dispatchMessage.Action}");
            return null;
        }

        var webhook = webhooks.First();
        // the response is used by the caller: a signed webhook must sign its response
        return await DispatchWebhook(context, tenant.Identifier, user.Identifier, webhook, dispatchMessage,
            functionRequest: true);
    }

    /// <inheritdoc />
    public async Task SendMessageAsync(IDbContext context, int tenantId, int userId, WebhookDispatchMessage dispatchMessage)
    {
        if (tenantId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tenantId));
        }
        if (userId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(userId));
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
        var user = await UserRepository.GetAsync(context, tenantId, userId);
        if (user == null)
        {
            throw new PayrollException($"Unknown user with id {userId}.");
        }

        // send message to any webhook
        foreach (var webhook in webhooks)
        {
            await DispatchWebhook(context, tenant.Identifier, user.Identifier, webhook, dispatchMessage);
        }
    }

    /// <summary>
    /// Dispatch a single webhook: build the runtime message, optionally persist tracking,
    /// POST to the receiver, and record the response.
    /// </summary>
    /// <remarks>
    /// Signed webhooks (a secret is set) send the headers <c>X-PE-Timestamp</c> and <c>X-PE-Signature</c>.
    /// A function request (<paramref name="functionRequest"/>) uses the response: a connection error or a failed status throws a
    /// <see cref="PayrollException"/> and a successful response of a signed webhook must carry a valid
    /// <c>X-PE-Response-Signature</c>, otherwise a <see cref="PayrollException"/> is thrown.
    /// Notifications ignore connection errors to ensure the execution of the remaining webhooks.
    /// </remarks>
    private async Task<string> DispatchWebhook(IDbContext context, string tenant, string user,
        Webhook webhook, WebhookDispatchMessage dispatchMessage, bool functionRequest = false)
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

        // tracking by server configuration: notifications and optionally function requests
        var track = functionRequest ? Tracking == WebhookTracking.All : Tracking != WebhookTracking.None;

        // track message start
        if (track)
        {
            // create webhook message
            await MessageRepository.CreateAsync(context, webhook.Id, webhookMessage);
        }

        // post - each call gets a fresh HttpClient from the factory,
        // ensuring proper DNS rotation and connection pooling
        Log.Debug($"Sending web hook {webhook.Name} to {webhook.ReceiverAddress}");
        var jsonMessage = DefaultJsonSerializer.Serialize(webhookMessage);

        // signature: a missing key must fail loudly and not send an unsigned message
        string secret = null;
        string requestSignature = null;
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (webhook.Signed)
        {
            try
            {
                secret = SecretProtector.Unprotect(webhook.Secret);
            }
            catch (Exception exception)
            {
                throw new PayrollException(
                    $"The secret of the webhook {webhook.Name} cannot be read: {exception.GetBaseMessage()}", exception);
            }
            requestSignature = WebhookSignature.SignRequest(secret, timestamp, jsonMessage);
        }

        var responseSuccess = false;
        string connectionError = null;
        string responseSignature = null;
        try
        {
            var httpClient = HttpClientFactory.CreateClient(HttpClientName);
            using var request = new HttpRequestMessage(HttpMethod.Post, webhook.ReceiverAddress);
            request.Content = DefaultJsonSerializer.SerializeJson(jsonMessage);
            if (requestSignature != null)
            {
                request.Headers.Add(WebhookSignature.TimestampHeader, timestamp.ToString(CultureInfo.InvariantCulture));
                request.Headers.Add(WebhookSignature.SignatureHeader, requestSignature);
            }
            using var response = await httpClient.SendAsync(request);
            webhookMessage.ResponseStatus = (int)response.StatusCode;
            if (response.IsSuccessStatusCode)
            {
                webhookMessage.ResponseMessage = await response.Content.ReadAsStringAsync();
                responseSuccess = true;
                if (response.Headers.TryGetValues(WebhookSignature.ResponseSignatureHeader, out var values))
                {
                    responseSignature = values.FirstOrDefault();
                }
            }
        }
        catch (Exception exception)
        {
            var errorMessage = exception.GetBaseMessage();
            Log.Error(exception, errorMessage);
            webhookMessage.ResponseMessage = errorMessage;
            connectionError = errorMessage;
        }

        // webhook response
        // ignore failed status to ensure execution of remaining webhooks
        webhookMessage.ResponseDate = Date.Now;

        // track message end
        if (track)
        {
            // update webhook message
            await MessageRepository.UpdateAsync(context, webhook.Id, webhookMessage);
        }

        Log.Debug($"completed web hook {webhook.Name} with response status {webhookMessage.ResponseStatus}");

        // the caller uses the response: an unreachable receiver must not yield a bogus value
        if (functionRequest && connectionError != null)
        {
            var message = $"The webhook {webhook.Name} cannot be reached at {webhook.ReceiverAddress}: {connectionError}";
            Log.Error(message);
            throw new PayrollException(message);
        }

        // a failed response status must not yield a bogus value either
        if (functionRequest && !responseSuccess)
        {
            var message = $"The webhook {webhook.Name} at {webhook.ReceiverAddress} returned the status {webhookMessage.ResponseStatus}";
            Log.Error(message);
            throw new PayrollException(message);
        }

        // the response of a signed webhook must be authentic: fail instead of using a forged value
        if (functionRequest && secret != null && responseSuccess &&
            !WebhookSignature.VerifyResponse(secret, timestamp, requestSignature,
                webhookMessage.ResponseMessage, responseSignature))
        {
            var message = string.IsNullOrWhiteSpace(responseSignature)
                ? $"Missing response signature of the webhook {webhook.Name}: header {WebhookSignature.ResponseSignatureHeader} not received (receiver without secret?)"
                : $"Invalid response signature of the webhook {webhook.Name}: secret mismatch or modified response";
            Log.Error(message);
            throw new PayrollException(message);
        }

        return webhookMessage.ResponseMessage;
    }

    private async Task<List<Webhook>> QueryInvokeWebhooks(IDbContext context, int tenantId, WebhookAction action)
    {
        var key = (tenantId, action);
        var query = invokeWebhooks.GetOrAdd(key, _ => QueryWebhooks(context, tenantId, action));
        try
        {
            return await query;
        }
        catch
        {
            // do not cache a failed query
            invokeWebhooks.TryRemove(new KeyValuePair<(int, WebhookAction), Task<List<Webhook>>>(key, query));
            throw;
        }
    }

    private async Task<List<Webhook>> QueryWebhooks(IDbContext context, int tenantId, WebhookAction action) =>
        (await WebhookRepository.QueryAsync(context, tenantId, new()
        {
            Status = ObjectStatus.Active,
            Filter = $"{nameof(Webhook.Action)} eq '{action}'"
        })).ToList();
}
