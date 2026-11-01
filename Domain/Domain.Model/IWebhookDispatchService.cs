using System.Threading.Tasks;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Webhook dispatcher service
/// </summary>
public interface IWebhookDispatchService
{
    /// <summary>
    /// Test for available webhooks
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="tenantId">The tenant id</param>
    Task<bool> HasWebhooksAsync(IDbContext context, int tenantId);

    /// <summary>
    /// Invoke the webhook synchronously and receive the response object.
    /// The caller passes the already loaded tenant and user, so nothing is reloaded per invocation.
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="tenant">The tenant</param>
    /// <param name="user">The user</param>
    /// <param name="dispatchMessage">The dispatch message</param>
    /// <returns>The webhook response object as JSON</returns>
    Task<string> InvokeAsync(IDbContext context, Tenant tenant, User user,
        WebhookDispatchMessage dispatchMessage);

    /// <summary>
    /// Send a notification message to all matching webhooks, without waiting for a result.
    /// The tenant and user are loaded by id, once per call.
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="userId">The user id</param>
    /// <param name="dispatchMessage">The dispatch message</param>
    System.Threading.Tasks.Task SendMessageAsync(IDbContext context, int tenantId, int userId,
        WebhookDispatchMessage dispatchMessage);
}