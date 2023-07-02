using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

namespace PayrollEngine.Api.Core;

[ApiController]
[Produces(ContentType.Json)]
public abstract class ApiController : ControllerBase
{
    public IControllerRuntime Runtime { get; }
    public IConfiguration Configuration => Runtime.Configuration;
    protected LinkGenerator LinkGenerator => Runtime.LinkGenerator;
    private IApiDescriptionGroupCollectionProvider ApiExplorer => Runtime.ApiExplorer;

    protected ApiController(IControllerRuntime runtime)
    {
        Runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
    }

    protected ActionResult InternalServerError(Exception exception) =>
        ActionResultFactory.InternalServerError(exception);

    #region Options

    [HttpOptions]
    [OkResponse]
    [ApiOperationId(nameof(GetOptions))]
    public IActionResult GetOptions()
    {
        // requested controller
        var requestController = RouteData.Values["controller"] as string;

        // find supported methods using the api explorer
        // https://stackoverflow.com/a/36454275
        var supportedMethods = new List<string> { "OPTIONS" };
        foreach (var descriptionGroup in ApiExplorer.ApiDescriptionGroups.Items)
        {
            foreach (var groupItem in descriptionGroup.Items)
            {
                if (groupItem.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
                {
                    var controller = controllerActionDescriptor.ControllerName;
                    if (string.Equals(requestController, controller, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!supportedMethods.Contains(groupItem.HttpMethod))
                        {
                            supportedMethods.Add(groupItem.HttpMethod);
                        }
                    }
                }
            }
        }

        if (supportedMethods.Count == 0)
        {
            return NotFound();
        }

        Response.Headers.Add("Access-Control-Allow-Origin", "*");
        Response.Headers.Add("Access-Control-Allow-Methods", string.Join(",", supportedMethods));
        return Ok();
    }

    #endregion

    #region Tenant

    protected BadRequestObjectResult VerifyTenant(int tenantId)
    {
        if (!Runtime.Tenant.IsValid(tenantId))
        {
            return BadRequest($"Invalid tenant with id {tenantId}");
        }
        return null;
    }

    #endregion
}