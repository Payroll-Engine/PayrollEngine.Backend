using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Tasks")]
[Route("api/tenants/{tenantId}/tasks")]
public class TaskController : Api.Controller.TaskController
{
    /// <inheritdoc/>
    public TaskController(ITenantService tenantService, ITaskService taskService,
        IUserService userService, IWebhookDispatchService webhookDispatcher, IControllerRuntime runtime) :
        base(tenantService, taskService, userService, webhookDispatcher, runtime)
    {
    }

    /// <summary>
    /// Query tasks
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The tenant tasks</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryTasks")]
    public async Task<ActionResult> QueryTasksAsync(int tenantId, [FromQuery] Query query)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await QueryItemsAsync(tenantId, query);
    }

    /// <summary>
    /// Get a task
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="taskId">The id of the task</param>
    /// <returns>The tenant task</returns>
    [HttpGet("{taskId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetTask")]
    public async Task<ActionResult<ApiObject.Task>> GetTaskAsync(int tenantId, int taskId)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await GetAsync(tenantId, taskId);
    }

    /// <summary>
    /// Add a new task
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="task">The task to add</param>
    /// <returns>The newly created task</returns>
    [HttpPost]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("CreateTask")]
    public async Task<ActionResult<ApiObject.Task>> CreateTaskAsync(int tenantId, ApiObject.Task task)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await CreateAsync(tenantId, task);
    }

    /// <summary>
    /// Update a task
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="task">The task with updated values</param>
    /// <returns>The modified task</returns>
    [HttpPut("{taskId}")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("UpdateTask")]
    public async Task<ActionResult<ApiObject.Task>> UpdateTaskAsync(int tenantId, ApiObject.Task task)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await UpdateAsync(tenantId, task);
    }

    /// <summary>
    /// Delete a task
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="taskId">The id of the task</param>
    [HttpDelete("{taskId}")]
    [ApiOperationId("DeleteTask")]
    public async Task<IActionResult> DeleteTaskAsync(int tenantId, int taskId)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await DeleteAsync(tenantId, taskId);
    }

    #region Attributes

    /// <summary>
    /// Get a task attribute
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="taskId">The id of the task</param>
    /// <param name="attributeName">The attribute name</param>
    /// <returns>The attribute value as JSON</returns>
    [HttpGet("{taskId}/attributes/{attributeName}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetTaskAttribute")]
    public virtual async Task<ActionResult<string>> GetTaskAttributeAsync(
        int tenantId, int taskId, string attributeName)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await GetAttributeAsync(taskId, attributeName);
    }

    /// <summary>
    /// Set a task attribute
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="taskId">The id of the task</param>
    /// <param name="attributeName">The attribute name</param>
    /// <param name="value">The attribute value as JSON</param>
    /// <returns>The current attribute value as JSON</returns>
    [HttpPost("{taskId}/attributes/{attributeName}")]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("SetTaskAttribute")]
    public virtual async Task<ActionResult<string>> SetTaskAttributeAsync(
        int tenantId, int taskId, string attributeName,
        [FromBody] string value)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await SetAttributeAsync(taskId, attributeName, value);
    }

    /// <summary>
    /// Delete a task attribute
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="taskId">The id of the task</param>
    /// <param name="attributeName">The attribute name</param>
    /// <returns>True if the attribute was deleted</returns>
    [HttpDelete("{taskId}/attributes/{attributeName}")]
    [ApiOperationId("DeleteTaskAttribute")]
    public virtual async Task<ActionResult<bool>> DeleteTaskAttributeAsync(
        int tenantId, int taskId, string attributeName)
    {
        // authorization
        var authResult = await TenantRequestAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await DeleteAttributeAsync(taskId, attributeName);
    }

    #endregion

}