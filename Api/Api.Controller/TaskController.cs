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
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for tasks
/// </summary>
[ApiControllerName("Tasks")]
[Route("api/tenants/{tenantId}/tasks")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.Task)]
public abstract class TaskController : RepositoryChildObjectController<ITenantService, ITaskService,
    ITenantRepository, ITaskRepository,
    Tenant, DomainObject.Task, ApiObject.Task>
{
    public IUserService UserService { get; }
    public IWebhookDispatchService WebhookDispatcher { get; }

    protected TaskController(ITenantService tenantService, ITaskService taskService,
        IUserService userService, IWebhookDispatchService webhookDispatcher, IControllerRuntime runtime) :
        base(tenantService, taskService, runtime, new TaskMap())
    {
        UserService = userService ?? throw new ArgumentNullException(nameof(userService));
        WebhookDispatcher = webhookDispatcher ?? throw new ArgumentNullException(nameof(webhookDispatcher));
    }

    protected override async Task<ActionResult<ApiObject.Task>> CreateAsync(int tenantId, ApiObject.Task task)
    {
        // validate user references
        if (!await UserService.ExistsAsync(Runtime.DbContext, tenantId, task.ScheduledUserId))
        {
            return BadRequest($"Task {task.Name} has unknown schedule user id {task.ScheduledUserId}");
        }
        if (task.CompletedUserId.HasValue &&
            !await UserService.ExistsAsync(Runtime.DbContext, tenantId, task.CompletedUserId.Value))
        {
            return BadRequest($"Task {task.Name} has unknown complete user id {task.CompletedUserId.Value}");
        }

        // create task
        var create = await base.CreateAsync(tenantId, task);

        // webhook
        if (create.Value != null)
        {
            var json = DefaultJsonSerializer.Serialize(create.Value);
            await WebhookDispatcher.SendMessageAsync(Runtime.DbContext, tenantId,
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
        // validate user references
        if (!await UserService.ExistsAsync(Runtime.DbContext, tenantId, task.ScheduledUserId))
        {
            return BadRequest($"Task {task.Name} has unknown schedule user id {task.ScheduledUserId}");
        }
        if (task.CompletedUserId.HasValue &&
            !await UserService.ExistsAsync(Runtime.DbContext, tenantId, task.CompletedUserId.Value))
        {
            return BadRequest($"Task {task.Name} has unknown complete user id {task.CompletedUserId.Value}");
        }

        var existing = await GetAsync(tenantId, task.Id);
        var update = await base.UpdateAsync(tenantId, task);

        // webhook in case of any date changes
        if (existing.Value != null && update.Value != null &&
            (existing.Value.Scheduled != update.Value.Scheduled ||
             existing.Value.Completed != update.Value.Completed))
        {
            var json = DefaultJsonSerializer.Serialize(update.Value);
            await WebhookDispatcher.SendMessageAsync(Runtime.DbContext, tenantId,
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