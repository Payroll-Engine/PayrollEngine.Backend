using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Users")]
[Route("api/tenants/{tenantId}/users")]
public class UserController : Api.Controller.UserController
{
    /// <inheritdoc/>
    public UserController(ITenantService tenantService, IUserService userService,
        IControllerRuntime runtime) :
        base(tenantService, userService, runtime)
    {
    }

    /// <summary>
    /// Query users
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The tenant users</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryUsers")]
    public async Task<ActionResult> QueryUsersAsync(int tenantId, [FromQuery] Query query)
    {
        // authorization
        var authResult = await AuthorizeAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await QueryItemsAsync(tenantId, query);
    }

    /// <summary>
    /// Get a user
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="userId">The id of the user</param>
    /// <returns></returns>
    [HttpGet("{userId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetUser")]
    public async Task<ActionResult<ApiObject.User>> GetUserAsync(int tenantId, int userId)
    {
        // authorization
        var authResult = await AuthorizeAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await GetAsync(tenantId, userId);
    }

    /// <summary>
    /// Add a new user
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="user">The user to add</param>
    /// <returns>The newly created user</returns>
    [HttpPost]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("CreateUser")]
    public async Task<ActionResult<ApiObject.User>> CreateUserAsync(int tenantId, ApiObject.User user)
    {
        // authorization
        var authResult = await AuthorizeAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        // unique user by identifier
        if (await Service.ExistsAnyAsync(Runtime.DbContext, tenantId, user.Identifier))
        {
            return BadRequest($"User with identifier {user.Identifier} already exists");
        }
        return await CreateAsync(tenantId, user);
    }

    /// <summary>
    /// Update a user
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="user">The user with updated values</param>
    /// <returns>The modified user</returns>
    [HttpPut("{userId}")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("UpdateUser")]
    public async Task<ActionResult<ApiObject.User>> UpdateUserAsync(int tenantId, ApiObject.User user)
    {
        // authorization
        var authResult = await AuthorizeAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await UpdateAsync(tenantId, user);
    }

    /// <summary>
    /// Delete a user
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="userId">The id of the user</param>
    /// <returns></returns>
    [HttpDelete("{userId}")]
    [ApiOperationId("DeleteUser")]
    public async Task<IActionResult> DeleteUserAsync(int tenantId, int userId)
    {
        // authorization
        var authResult = await AuthorizeAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await DeleteAsync(tenantId, userId);
    }

    #region Password

    /// <summary>
    /// Test the user password
    /// </summary>
    /// <remarks>
    /// Request body contains array of case values (optional)
    /// Without the request body, this would be a GET method
    /// </remarks>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="userId">The id of the user</param>
    /// <param name="password">The password to test</param>
    /// <returns>True for a valid password</returns>
    [HttpPost("{userId}/password")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("TestUserPassword")]
    [QueryIgnore]
    public async Task<ActionResult> TestUserPasswordAsync(int tenantId, int userId, [FromBody] string password)
    {
        // authorization
        var authResult = await AuthorizeAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }

        // password
        if (string.IsNullOrWhiteSpace(password))
        {
            return BadRequest("Missing password");
        }

        // user
        var user = await Service.GetAsync(Runtime.DbContext, tenantId, userId);
        if (user == null)
        {
            return ObjectNotFoundRequest(userId);
        }

        // test password
        try
        {
            var valid = await Service.TestPasswordAsync(Runtime.DbContext, tenantId, userId, password);
            return valid ? Ok() : BadRequest("Invalid password");
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    /// <summary>
    /// Update the user password
    /// Change request use cases:
    /// - set initial password: new=required, existing=ignored
    /// - change existing password: new=required, existing=required
    /// - reset password: new=null, existing=required
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="userId">The id of the user</param>
    /// <param name="changeRequest">The password change request including the new and existing password</param>
    /// <returns>The updated user</returns>
    [HttpPut("{userId}/password")]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("UpdateUserPassword")]
    public async Task<ActionResult<ApiObject.User>> UpdateUserPasswordAsync(
        int tenantId, int userId, [FromBody] PasswordChangeRequest changeRequest)
    {
        // authorization
        var authResult = await AuthorizeAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }

        // password
        if (string.IsNullOrWhiteSpace(changeRequest.NewPassword))
        {
            return Ok(false);
        }

        // update password
        try
        {
            await Service.UpdatePasswordAsync(Runtime.DbContext, tenantId, userId, changeRequest);
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }

        // updated user
        return await GetUserAsync(tenantId, userId);
    }

    #endregion

    #region Attributes

    /// <summary>
    /// Get a user attribute
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="userId">The id of the user</param>
    /// <param name="attributeName">The attribute name</param>
    /// <returns>The attribute value as JSON</returns>
    [HttpGet("{userId}/attributes/{attributeName}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetUserAttribute")]
    public virtual async Task<ActionResult<string>> GetUserAttributeAsync(
        int tenantId, int userId, string attributeName)
    {
        // authorization
        var authResult = await AuthorizeAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await GetAttributeAsync(userId, attributeName);
    }

    /// <summary>
    /// Set a user attribute
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="userId">The id of the user</param>
    /// <param name="attributeName">The attribute name</param>
    /// <param name="value">The attribute value as JSON</param>
    /// <returns>The current attribute value as JSON</returns>
    [HttpPost("{userId}/attributes/{attributeName}")]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("SetUserAttribute")]
    public virtual async Task<ActionResult<string>> SetUserAttributeAsync(
        int tenantId, int userId, string attributeName, [FromBody] string value)
    {
        // authorization
        var authResult = await AuthorizeAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await SetAttributeAsync(userId, attributeName, value);
    }

    /// <summary>
    /// Delete a user attribute
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="userId">The id of the user</param>
    /// <param name="attributeName">The attribute name</param>
    /// <returns>True if the attribute was deleted</returns>
    [HttpDelete("{userId}/attributes/{attributeName}")]
    [ApiOperationId("DeleteUserAttribute")]
    public virtual async Task<ActionResult<bool>> DeleteUserAttributeAsync(
        int tenantId, int userId, string attributeName)
    {
        // authorization
        var authResult = await AuthorizeAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await DeleteAttributeAsync(userId, attributeName);
    }

    #endregion

}