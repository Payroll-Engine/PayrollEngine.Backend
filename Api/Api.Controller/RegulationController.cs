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
public abstract class RegulationController(ITenantService tenantService, IRegulationService regulationService,
        ICaseService caseService, ICaseFieldService caseFieldService, IControllerRuntime runtime)
    : RepositoryChildObjectController<ITenantService, IRegulationService,
    ITenantRepository, IRegulationRepository,
    Tenant, Regulation, ApiObject.Regulation>(tenantService, regulationService, runtime, new RegulationMap())
{
    private ICaseService CaseService { get; } = caseService ?? throw new ArgumentNullException(nameof(caseService));
    private ICaseFieldService CaseFieldService { get; } = caseFieldService ?? throw new ArgumentNullException(nameof(caseFieldService));

    public virtual async Task<ActionResult<string>> GetCaseOfCaseFieldAsync(int tenantId, string caseFieldName)
    {
        try
        {
            // tenant
            var authResult = await TenantRequestAsync(tenantId);
            if (authResult != null)
            {
                return authResult;
            }

            // case field
            var caseFields = await CaseFieldService.GetRegulationCaseFieldsAsync(Runtime.DbContext, tenantId,
                [caseFieldName]);
            var caseFieldId = caseFields.FirstOrDefault()?.Id;
            if (!caseFieldId.HasValue)
            {
                return BadRequest($"Unknown case field {caseFieldName}");
            }

            // case id
            var caseId = await CaseFieldService.GetParentIdAsync(Runtime.DbContext, caseFieldId.Value);
            if (!caseId.HasValue || caseId == 0)
            {
                return BadRequest($"Unknown case for case field {caseFieldName}");
            }

            // regulation id
            var regulationId = await CaseService.GetParentIdAsync(Runtime.DbContext, caseId.Value);
            if (!regulationId.HasValue || regulationId == 0)
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