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
    protected ILookupSetService LookupSetService { get; }
    protected ICaseService CaseService { get; }
    protected ICaseFieldService CaseFieldService { get; }
    protected ICaseRelationService CaseRelationService { get; }

    protected RegulationController(ITenantService tenantService, ILookupSetService lookupSetService, IRegulationService regulationService,
        ICaseService caseService, ICaseFieldService caseFieldService, ICaseRelationService caseRelationService, IControllerRuntime runtime) :
        base(tenantService, regulationService, runtime, new RegulationMap())
    {
        LookupSetService = lookupSetService ?? throw new ArgumentNullException(nameof(lookupSetService));
        CaseService = caseService ?? throw new ArgumentNullException(nameof(caseService));
        CaseFieldService = caseFieldService ?? throw new ArgumentNullException(nameof(caseFieldService));
        CaseRelationService = caseRelationService ?? throw new ArgumentNullException(nameof(caseRelationService));
    }

    public virtual async Task<ActionResult<string>> GetCaseOfCaseFieldAsync(int tenantId, string caseFieldName)
    {
        try
        {
            // tenant
            var tenantResult = VerifyTenant(tenantId);
            if (tenantResult != null)
            {
                return tenantResult;
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