using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
public class ReportController : Api.Controller.ReportController
{
    /// <inheritdoc/>
    public ReportController(ITenantService tenantService, IRegulationService regulationService, IReportService reportService,
        IReportSetService reportSetService, IControllerRuntime runtime) :
        base(tenantService, regulationService, reportService, reportSetService, runtime)
    {
    }

    /// <summary>
    /// Query reports
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The regulation reports</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryReports")]
    public async Task<ActionResult> QueryReportsAsync(int tenantId, int regulationId, [FromQuery] Query query)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await QueryItemsAsync(regulationId, query);
    }

    /// <summary>
    /// Get a report
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="reportId">The id of the report</param>
    /// <returns>The regulation report</returns>
    [HttpGet("{reportId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetReport")]
    public async Task<ActionResult<ApiObject.Report>> GetReportAsync(int tenantId, int regulationId, int reportId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await GetAsync(regulationId, reportId);
    }

    /// <summary>
    /// Execute a report
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="reportId">The id of the report</param>
    /// <param name="request">The report execute request</param>
    /// <returns>The report response including the report data</returns>
    [HttpGet("{reportId}/execute")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("ExecuteReport")]
    [QueryIgnore]
    public override async Task<ActionResult<ApiObject.ReportResponse>> ExecuteReportAsync(int tenantId, int regulationId,
        int reportId, [FromBody][Required] ApiObject.ReportRequest request)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await base.ExecuteReportAsync(tenantId, regulationId, reportId, request);
    }

    /// <summary>
    /// Add a new report
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="report">The report to add</param>
    /// <returns>The newly created report</returns>
    [HttpPost]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("CreateReport")]
    public async Task<ActionResult<ApiObject.Report>> CreateReportAsync(int tenantId, int regulationId,
        ApiObject.Report report)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await CreateAsync(regulationId, report);
    }

    /// <summary>
    /// Update a report
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="report">The report with updated values</param>
    /// <returns>The modified report</returns>
    [HttpPut("{reportId}")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("UpdateReport")]
    public async Task<ActionResult<ApiObject.Report>> UpdateReportAsync(int tenantId, int regulationId, ApiObject.Report report)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await UpdateAsync(regulationId, report);
    }

    /// <summary>
    /// Delete a report
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="reportId">The id of the report</param>
    /// <returns></returns>
    [HttpDelete("{reportId}")]
    [ApiOperationId("DeleteReport")]
    public async Task<IActionResult> DeleteReportAsync(int tenantId, int regulationId, int reportId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await DeleteAsync(regulationId, reportId);
    }

    #region Report Set

    /// <summary>
    /// Get a report set
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The tenant id</param>
    /// <param name="reportId">The id of the report</param>
    /// <param name="reportRequest">The report execute request</param>
    /// <returns>The regulation report set</returns>
    [HttpGet("sets/{reportId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetReportSet")]
    public override async Task<ActionResult<ApiObject.ReportSet>> GetReportSetAsync(int tenantId, int regulationId,
        int reportId, [FromBody][Required] ApiObject.ReportRequest reportRequest)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await base.GetReportSetAsync(tenantId, regulationId, reportId, reportRequest);
    }

    /// <summary>
    /// Add a new report set
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The tenant id</param>
    /// <param name="report">The report set to add</param>
    /// <returns>The newly created report set</returns>
    [HttpPost("sets")]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("CreateReportSet")]
    public async Task<ActionResult<ApiObject.ReportSet>> CreateReportSetAsync(int tenantId, int regulationId,
        ApiObject.ReportSet report)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await base.CreateReportSetAsync(regulationId, report);
    }

    /// <summary>
    /// Rebuild regulation report
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The regulation id</param>
    /// <param name="reportId">The id of the report</param>
    [HttpPut("{reportId}/rebuild")]
    [NotFoundResponse]
    [ApiOperationId("RebuildReport")]
    public async Task<ActionResult> RebuildReportAsync(int tenantId, int regulationId, int reportId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await RebuildAsync(regulationId, reportId);
    }

    /// <summary>
    /// Delete a report set
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The tenant id</param>
    /// <param name="reportId">The id of the report</param>
    [HttpDelete("sets/{reportId}")]
    [ApiOperationId("DeleteReportSet")]
    public async Task<IActionResult> DeleteReportSetAsync(int tenantId, int regulationId, int reportId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await DeleteReportSetAsync(regulationId, reportId);
    }

    #endregion

    #region Attributes

    /// <summary>
    /// Get a report attribute
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The tenant id</param>
    /// <param name="reportId">The id of the report</param>
    /// <param name="attributeName">The attribute name</param>
    /// <returns>The attribute value as JSON</returns>
    [HttpGet("{reportId}/attributes/{attributeName}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetReportAttribute")]
    public virtual async Task<ActionResult<string>> GetReportAttributeAsync(int tenantId, int regulationId, int reportId, string attributeName)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await GetAttributeAsync(reportId, attributeName);
    }

    /// <summary>
    /// Set a report attribute
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The tenant id</param>
    /// <param name="reportId">The id of the report</param>
    /// <param name="attributeName">The attribute name</param>
    /// <param name="value">The attribute value as JSON</param>
    /// <returns>The current attribute value as JSON</returns>
    [HttpPost("{reportId}/attributes/{attributeName}")]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("SetReportAttribute")]
    public virtual async Task<ActionResult<string>> SetReportAttributeAsync(int tenantId, int regulationId,
        int reportId, string attributeName, [FromBody] string value)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await base.SetAttributeAsync(reportId, attributeName, value);
    }

    /// <summary>
    /// Delete a report attribute
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="regulationId">The tenant id</param>
    /// <param name="reportId">The id of the report</param>
    /// <param name="attributeName">The attribute name</param>
    /// <returns>True if the attribute was deleted</returns>
    [HttpDelete("{reportId}/attributes/{attributeName}")]
    [ApiOperationId("DeleteReportAttribute")]
    public virtual async Task<ActionResult<bool>> DeleteReportAttributeAsync(int tenantId, int regulationId, int reportId, string attributeName)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await base.DeleteAttributeAsync(reportId, attributeName);
    }

    #endregion

}