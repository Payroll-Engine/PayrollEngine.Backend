using System;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for payroll regulations
/// </summary>
public abstract class RegulationController : RepositoryChildObjectController<ITenantService, IRegulationService,
    ITenantRepository, IRegulationRepository,
    Tenant, Regulation, ApiObject.Regulation>
{
    private ICaseService CaseService { get; }
    private ICaseFieldService CaseFieldService { get; }

    protected RegulationController(ITenantService tenantService, IRegulationService regulationService,
        ICaseService caseService, ICaseFieldService caseFieldService, IControllerRuntime runtime) :
        base(tenantService, regulationService, runtime, new RegulationMap())
    {
        CaseService = caseService ?? throw new ArgumentNullException(nameof(caseService));
        CaseFieldService = caseFieldService ?? throw new ArgumentNullException(nameof(caseFieldService));
    }

    public virtual async Task<ActionResult<string>> GetCaseOfCaseFieldAsync(int tenantId, string caseFieldName)
    {
        try
        {
            // tenant
            var authResult = await AuthorizeAsync(tenantId);
            if(authResult != null)
            {
                return authResult;
            }

            // case field
            var caseFields = await CaseFieldService.GetRegulationCaseFieldsAsync(Runtime.DbContext, tenantId, new[] { caseFieldName });
            var caseId = caseFields.FirstOrDefault()?.Id;
            if (!caseId.HasValue)
            {
                return BadRequest($"Unknown case field {caseFieldName}");
            }

            var regulationId = await CaseService.GetParentIdAsync(Runtime.DbContext, caseId.Value);
            if (!regulationId.HasValue)
            {
                return BadRequest($"Unknown case for case field {caseFieldName}");
            }

            // case
            var @case = await CaseService.GetAsync(Runtime.DbContext, regulationId.Value, caseId.Value);
            return @case.Name;
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }
}