using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
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
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
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
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
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
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        // unique user by identifier
        if (await Service.ExistsAnyAsync(tenantId, user.Identifier))
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
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
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
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await DeleteAsync(tenantId, userId);
    }

    #region Password

    /// <summary>
    /// Test the user password
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="userId">The id of the user</param>
    /// <param name="password">The password to test</param>
    /// <returns>True for a valid password</returns>
    [HttpGet("{userId}/password")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("TestUserPassword")]
    [QueryIgnore]
    public async Task<ActionResult> TestUserPasswordAsync(int tenantId, int userId, [FromBody] string password)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }

        // password
        if (string.IsNullOrWhiteSpace(password))
        {
            return BadRequest("Missing password");
        }

        // user
        var user = await Service.GetAsync(tenantId, userId);
        if (user == null)
        {
            return ObjectNotFoundRequest(userId);
        }

        // compare password
        var existingHashSalt = new HashSalt(user.Password, user.StoredSalt);
        var testHashSalt = password.ToHashSalt(user.StoredSalt);
        return existingHashSalt.Equals(testHashSalt) ? Ok() : BadRequest("Invalid password");
    }

    /// <summary>
    /// Update the user password
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="userId">The id of the user</param>
    /// <param name="password">The new user password, use null to reset the password</param>
    /// <returns>The updated user</returns>
    [HttpPost("{userId}/password")]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("UpdateUserPassword")]
    public async Task<ActionResult<ApiObject.User>> UpdateUserPasswordAsync(int tenantId, int userId, [FromBody] string password)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }

        // password
        if (string.IsNullOrWhiteSpace(password))
        {
            return Ok(false);
        }

        // update password
        try
        {
            await Service.UpdatePasswordAsync(tenantId, userId, password);
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
    public virtual async Task<ActionResult<string>> GetUserAttributeAsync(int tenantId, int userId, string attributeName)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
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
    public virtual async Task<ActionResult<string>> SetUserAttributeAsync(int tenantId, int userId, string attributeName, [FromBody] string value)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await base.SetAttributeAsync(userId, attributeName, value);
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
    public virtual async Task<ActionResult<bool>> DeleteUserAttributeAsync(int tenantId, int userId, string attributeName)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await base.DeleteAttributeAsync(userId, attributeName);
    }

    #endregion

}