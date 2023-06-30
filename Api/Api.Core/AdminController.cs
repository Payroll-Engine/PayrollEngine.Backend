using System;
using System.Collections.Generic;
using PayrollEngine.Domain.Scripting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace PayrollEngine.Api.Core;

public abstract class AdminController : ApiController
{
    private IHostApplicationLifetime ApplicationLifetime { get; }

    protected AdminController(IControllerRuntime runtime, IHostApplicationLifetime appLifetime) :
        base(runtime)
    {
        ApplicationLifetime = appLifetime ?? throw new ArgumentNullException(nameof(appLifetime));
    }

    /// <summary>
    /// Requests termination of the API application
    /// </summary>
    /// <remarks>
    /// In IIS the application will be restarted with the next API request
    /// source https://edi.wang/post/2019/3/7/restart-an-aspnet-core-application-programmatically
    /// </remarks>
    /// <returns>Ok</returns>
    public virtual ActionResult StopApplication()
    {
        Log.Information("Stopping the application");
        ApplicationLifetime.StopApplication();
        return Ok();
    }

    /// <summary>
    /// Requests termination of the API application
    /// </summary>
    /// <remarks>
    /// In IIS the application will be restarted with the next API request
    /// source https://edi.wang/post/2019/3/7/restart-an-aspnet-core-application-programmatically
    /// </remarks>
    /// <returns>Ok</returns>
    public virtual ActionResult ClearApplicationCache()
    {
        AssemblyCache.CacheClear();
        return Ok();
    }

    /// <summary>
    /// Get query method names
    /// </summary>
    /// <returns>List of web method names</returns>
    public virtual ActionResult<IEnumerable<string>> GetApiReportMethods()
    {
        return Ok(ApiQueryFactory.GetQueryNames());
    }
}