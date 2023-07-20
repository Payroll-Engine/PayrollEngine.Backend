using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;
// ReSharper disable UnusedParameter.Global

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Report parameters")]
[Route("api/tenants/{tenantId}/regulations/{regulationId}/reports/{reportId}/parameters")]
public class ReportParameterController : Api.Controller.ReportParameterController
{
    /// <inheritdoc/>
    public ReportParameterController(IReportService reportService, IReportParameterService reportParameterService,
        IControllerRuntime runtime) :
        base(reportService, reportParameterService, runtime)
    {
    }

    /// <summary>
    /// Query report parameters
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The tenant id</param>
    /// <param name="reportId">The report id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The report parameters</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryReportParameters")]
    public async Task<ActionResult> QueryReportParametersAsync(int tenantId, int regulationId, int reportId, [FromQuery] Query query)
    {
        // authorization
        var authResult = await AuthorizeAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await QueryItemsAsync(reportId, query);
    }

    /// <summary>
    /// Get a report parameter
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The tenant id</param>
    /// <param name="reportId">The report id</param>
    /// <param name="parameterId">The id of the parameter</param>
    /// <returns>The report parameter</returns>
    [HttpGet("{parameterId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetReportParameter")]
    public async Task<ActionResult<ApiObject.ReportParameter>> GetReportParameterAsync(
        int tenantId, int regulationId, int reportId, int parameterId)
    {
        // authorization
        var authResult = await AuthorizeAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await GetAsync(reportId, parameterId);
    }

    /// <summary>
    /// Add a new report parameter
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The tenant id</param>
    /// <param name="reportId">The report id</param>
    /// <param name="parameter">The report parameter to add</param>
    /// <returns>The newly created report parameter</returns>
    [HttpPost]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("CreateReportParameter")]
    public async Task<ActionResult<ApiObject.ReportParameter>> CreateReportParameterAsync(
        int tenantId, int regulationId, int reportId, ApiObject.ReportParameter parameter)
    {
        // authorization
        var authResult = await AuthorizeAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await CreateAsync(reportId, parameter);
    }

    /// <summary>
    /// Update a report parameter
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The tenant id</param>
    /// <param name="reportId">The report id</param>
    /// <param name="parameter">The report parameter to modify</param>
    /// <returns>The modified parameter</returns>
    [HttpPut("{parameterId}")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("UpdateReportParameter")]
    public async Task<ActionResult<ApiObject.ReportParameter>> UpdateReportParameterAsync(
        int tenantId, int regulationId, int reportId, ApiObject.ReportParameter parameter)
    {
        // authorization
        var authResult = await AuthorizeAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await UpdateAsync(reportId, parameter);
    }

    /// <summary>
    /// Delete a report
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The tenant id</param>
    /// <param name="reportId">The report id</param>
    /// <param name="parameterId">The id of the report parameter</param>
    /// <returns></returns>
    [HttpDelete("{parameterId}")]
    [ApiOperationId("DeleteReportParameter")]
    public async Task<IActionResult> DeleteReportParameterAsync(int tenantId,
        int regulationId, int reportId, int parameterId)
    {
        // authorization
        var authResult = await AuthorizeAsync(tenantId);
        if(authResult != null)
        {
            return authResult;
        }
        return await DeleteAsync(reportId, parameterId);
    }
}