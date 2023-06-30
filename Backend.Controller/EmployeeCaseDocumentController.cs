using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Employee case documents")]
[Route("api/tenants/{tenantId}/employees/{employeeId}/cases/{caseValueId}/documents")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.EmployeeCaseDocument)]
public class EmployeeCaseDocumentController : Api.Controller.EmployeeCaseDocumentController
{
    /// <inheritdoc/>
    public EmployeeCaseDocumentController(IEmployeeCaseValueService caseValueService, IEmployeeCaseDocumentService caseDocumentService,
        IControllerRuntime runtime) :
        base(caseValueService, caseDocumentService, runtime)
    {
    }

    /// <summary>
    /// Query employee case documents
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="employeeId">The employee id</param>
    /// <param name="caseValueId">The case value id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The employee case documents</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryEmployeeCaseDocuments")]
    public async Task<ActionResult> QueryEmployeeCaseDocumentsAsync(int tenantId, int employeeId, 
        int caseValueId, [FromQuery] Query query)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }

        // employee check
        var parentEmployeeId = await ParentService.GetParentIdAsync(Runtime.DbContext, caseValueId);
        if (parentEmployeeId != employeeId)
        {
            return BadRequest($"Unknown employee with id {employeeId}");
        }

        // case value check
        if (!await ParentService.ExistsAsync(Runtime.DbContext, caseValueId))
        {
            return BadRequest($"Unknown case value with id {caseValueId}");
        }

        return await QueryItemsAsync(caseValueId, query);
    }

    /// <summary>
    /// Get an employee case document
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="employeeId">The employee id</param>
    /// <param name="caseValueId">The case value id</param>
    /// <param name="documentId">The document id</param>
    /// <returns>The employee case document</returns>
    [HttpGet("{documentId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetEmployeeCaseDocument")]
    public async Task<ActionResult<ApiObject.CaseDocument>> GetEmployeeCaseDocumentAsync(int tenantId, 
        int employeeId, int caseValueId, int documentId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }

        // employee check
        if (!await ParentService.ExistsAsync(Runtime.DbContext, employeeId))
        {
            return BadRequest($"Unknown employee with id {employeeId}");
        }

        // case value check
        if (!await Service.ExistsAsync(Runtime.DbContext, caseValueId))
        {
            return BadRequest($"Unknown case value with id {caseValueId}");
        }

        return await GetAsync(caseValueId, documentId);
    }
}