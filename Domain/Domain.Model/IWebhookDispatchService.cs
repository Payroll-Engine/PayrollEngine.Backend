﻿using System.Threading.Tasks;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Webhook dispatcher service
/// </summary>
public interface IWebhookDispatchService
{
    /// <summary>
    /// Invoke webhook and receive the response object
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="dispatchMessage">The dispatch message</param>
    /// <param name="userId">The user id</param>
    /// <returns>The webhook response object as JSON</returns>
    Task<string> InvokeAsync(IDbContext context, int tenantId,
        WebhookDispatchMessage dispatchMessage, int? userId = null);

    /// <summary>
    /// Send message to the webhook
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="dispatchMessage">The dispatch message</param>
    /// <param name="userId">The user id</param>
    System.Threading.Tasks.Task SendMessageAsync(IDbContext context, int tenantId, 
        WebhookDispatchMessage dispatchMessage, int? userId = null);
}