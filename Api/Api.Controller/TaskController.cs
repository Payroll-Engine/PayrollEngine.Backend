using System;
using System.Threading.Tasks;
using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for tasks
/// </summary>
[ApiControllerName("Tasks")]
[Route("api/tenants/{tenantId}/tasks")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.Task)]
public abstract class TaskController : RepositoryChildObjectController<ITenantService, ITaskService,
    ITenantRepository, ITaskRepository,
    DomainObject.Tenant, DomainObject.Task, ApiObject.Task>
{
    public DomainObject.IWebhookDispatchService WebhookDispatcher { get; }

    protected TaskController(ITenantService tenantService, ITaskService taskService,
        DomainObject.IWebhookDispatchService webhookDispatcher, IControllerRuntime runtime) :
        base(tenantService, taskService, runtime, new TaskMap())
    {
        WebhookDispatcher = webhookDispatcher ?? throw new ArgumentNullException(nameof(webhookDispatcher));
    }

    protected override async Task<ActionResult<ApiObject.Task>> CreateAsync(int tenantId, ApiObject.Task task)
    {
        var create = await base.CreateAsync(tenantId, task);

        // webhook
        if (create.Value != null)
        {
            var json = DefaultJsonSerializer.Serialize(create.Value);
            await WebhookDispatcher.SendMessageAsync(tenantId,
                new()
                {
                    Action = WebhookAction.TaskChange,
                    RequestMessage = json
                },
                userId: create.Value.ScheduledUserId);

        }
        return create;
    }

    protected override async Task<ActionResult<ApiObject.Task>> UpdateAsync(int tenantId, ApiObject.Task task)
    {
        var existing = await GetAsync(tenantId, task.Id);
        var update = await base.UpdateAsync(tenantId, task);

        // webhook in case of any date changes
        if (existing.Value != null && update.Value != null &&
            (existing.Value.Scheduled != update.Value.Scheduled ||
             existing.Value.Completed != update.Value.Completed))
        {
            var json = DefaultJsonSerializer.Serialize(update.Value);
            await WebhookDispatcher.SendMessageAsync(tenantId,
                new()
                {
                    Action = WebhookAction.TaskChange,
                    RequestMessage = json
                },
                userId: update.Value.CompletedUserId ?? update.Value.ScheduledUserId);
        }
        return update;
    }
}