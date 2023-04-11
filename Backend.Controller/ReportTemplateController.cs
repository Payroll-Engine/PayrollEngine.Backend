using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
public class ReportTemplateController : Api.Controller.ReportTemplateController
{
    /// <inheritdoc/>
    public ReportTemplateController(IReportService reportService, IReportTemplateService reportTemplateService,
        IControllerRuntime runtime) :
        base(reportService, reportTemplateService, runtime)
    {
    }

    /// <summary>
    /// Query report templates
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="reportId">The report id</param>
    /// <param name="query">Query templates</param>
    /// <returns>The report templates</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryReportTemplates")]
    public async Task<ActionResult> QueryReportTemplatesAsync(int tenantId, int reportId, [FromQuery] ReportTemplateQuery query)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await QueryItemsAsync(reportId, query);
    }

    /// <summary>
    /// Get a report template
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="reportId">The report id</param>
    /// <param name="templateId">The id of the template</param>
    /// <returns>The report template</returns>
    [HttpGet("{templateId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetReportTemplate")]
    public async Task<ActionResult<ApiObject.ReportTemplate>> GetReportTemplateAsync(int tenantId, int reportId, int templateId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await GetAsync(reportId, templateId);
    }

    /// <summary>
    /// Add a new report template
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="reportId">The report id</param>
    /// <param name="template">The report template to add</param>
    /// <returns>The newly created report template</returns>
    [HttpPost]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("CreateReportTemplate")]
    public async Task<ActionResult<ApiObject.ReportTemplate>> CreateReportTemplateAsync(int tenantId,
        int reportId, ApiObject.ReportTemplate template)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await CreateAsync(reportId, template);
    }

    /// <summary>
    /// Update a report template
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="reportId">The report id</param>
    /// <param name="template">The report template to modify</param>
    /// <returns>The modified template</returns>
    [HttpPut("{templateId}")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("UpdateReportTemplate")]
    public async Task<ActionResult<ApiObject.ReportTemplate>> UpdateReportTemplateAsync(int tenantId, int reportId, ApiObject.ReportTemplate template)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await UpdateAsync(reportId, template);
    }

    /// <summary>
    /// Delete a report
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="reportId">The report id</param>
    /// <param name="templateId">The id of the report template</param>
    /// <returns></returns>
    [HttpDelete("{templateId}")]
    [ApiOperationId("DeleteReportTemplate")]
    public async Task<IActionResult> DeleteReportTemplateAsync(int tenantId, int reportId, int templateId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await DeleteAsync(reportId, templateId);
    }
}