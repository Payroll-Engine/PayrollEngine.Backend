using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

namespace PayrollEngine.Api.Core;

[ApiController]
[Produces(ContentType.Json)]
public abstract class ApiController(IControllerRuntime runtime) : ControllerBase
{
    public IControllerRuntime Runtime { get; } = runtime ?? throw new ArgumentNullException(nameof(runtime));
    public IConfiguration Configuration => Runtime.Configuration;
    protected LinkGenerator LinkGenerator => Runtime.LinkGenerator;
    private IApiDescriptionGroupCollectionProvider ApiExplorer => Runtime.ApiExplorer;

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

        // CORS headers are handled by the ASP.NET Core CORS middleware
        // (configured in ApiStartupExtensions via CorsConfiguration).
        // No manual Access-Control-* headers needed here.
        Response.Headers.Append("Allow", string.Join(",", supportedMethods));
        return Ok();
    }

    #endregion

    #region Tenant

    protected async Task<UnauthorizedObjectResult> TenantRequestAsync(int tenantId)
    {
        // tenant authorization header
        Request.Headers.TryGetValue(BackendSpecification.TenantAuthorizationHeader, out var authTenant);

        // tenant
        var tenant = await Runtime.DbContext.GetTenantAsync(tenantId, authTenant);
        if (tenant == null)
        {
            return Unauthorized($"Invalid tenant with id {tenantId}");
        }

        // apply tenant culture to the current async context
        if (!string.IsNullOrWhiteSpace(tenant.Culture))
        {
            if (!string.Equals(CultureInfo.CurrentCulture.Name, tenant.Culture))
            {
                CultureInfo.CurrentCulture = new(tenant.Culture);
            }
        }
        return null;
    }

    #endregion
}