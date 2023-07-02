using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using PayrollEngine.Api.Core;

namespace PayrollEngine.Backend.Controller;

/// <summary>
/// The administration controller
/// </summary>
[ApiController]
[Produces(ContentType.Json)]
[ApiControllerName("Administration")]
[Route("api/admin")]
public class AdminController : PayrollEngine.Api.Core.AdminController
{
    /// <inheritdoc />
    public AdminController(IControllerRuntime runtime, IHostApplicationLifetime appLifetime) :
        base(runtime, appLifetime)
    {
    }

    /// <summary>
    /// Requests termination of the API application
    /// </summary>
    /// <remarks>
    /// In IIS the application will be restarted with the next API request
    /// source https://edi.wang/post/2019/3/7/restart-an-aspnet-core-application-programmatically
    /// </remarks>
    /// <returns>Ok</returns>
    [HttpPost("application/stop")]
    [OkResponse]
    [ApiOperationId("StopApplication")]
    public override ActionResult StopApplication() =>
        base.StopApplication();

    /// <summary>
    /// Clears the application cache
    /// </summary>
    /// <returns>Ok</returns>
    [HttpPost("application/clearcache")]
    [OkResponse]
    [ApiOperationId("ClearApplicationCache")]
    public override ActionResult ClearApplicationCache() =>
        base.ClearApplicationCache();

    /// <summary>
    /// Get the API report query method names (see TenantController.ExecuteReportQueryAsync)
    /// </summary>
    /// <returns>List of web method names</returns>
    [HttpGet("reportmethods")]
    [OkResponse]
    [ApiOperationId("GetApiReportMethods")]
    [QueryIgnore]
    public override ActionResult<IEnumerable<string>> GetApiReportMethods() =>
        base.GetApiReportMethods();
}