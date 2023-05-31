using System;
using System.Collections.Generic;
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
/// API controller for the regulation case relations
/// </summary>
public abstract class CaseRelationController : ScriptTrackChildObjectController<IRegulationService, ICaseRelationService,
    IRegulationRepository, ICaseRelationRepository, Regulation, CaseRelation, CaseRelationAudit, ApiObject.CaseRelation>
{
    private ICaseService CaseService { get; }

    protected CaseRelationController(IRegulationService regulationService, ICaseService caseService,
        ICaseRelationService caseRelationService, IControllerRuntime runtime) :
        base(regulationService, caseRelationService, runtime, new CaseRelationMap())
    {
        CaseService = caseService ?? throw new ArgumentNullException(nameof(caseService));
    }

    protected override async Task<ActionResult<ApiObject.CaseRelation>> CreateAsync(
        int regulationId, ApiObject.CaseRelation apiCaseRelation)
    {
        // self reference
        if (string.Equals(apiCaseRelation.SourceCaseName, apiCaseRelation.TargetCaseName))
        {
            return BadRequest($"Invalid relation to itself in case {regulationId}");
        }

        // tenant
        var tenantId = await ParentService.GetParentIdAsync(Runtime.DbContext, regulationId);
        if (!tenantId.HasValue)
        {
            return BadRequest($"Invalid regulation id {regulationId}");
        }

        // regulation
        if (!await ParentService.ExistsAsync(Runtime.DbContext, tenantId.Value, regulationId))
        {
            return BadRequest($"Unknown regulation id {regulationId}");
        }

        // source case
        var sourceCase = await CaseService.GetAsync(Runtime.DbContext, tenantId.Value, regulationId, apiCaseRelation.SourceCaseName);
        if (sourceCase != null)
        {
            apiCaseRelation.SourceCaseNameLocalizations = sourceCase.NameLocalizations;
            if (!string.IsNullOrWhiteSpace(apiCaseRelation.SourceCaseSlot))
            {
                var slot = sourceCase.Slots?.FirstOrDefault(x => string.Equals(x.Name, apiCaseRelation.SourceCaseSlot));
                if (slot != null)
                {
                    apiCaseRelation.SourceCaseSlotLocalizations = slot.NameLocalizations;
                }
            }
        }

        // target case
        var targetCase = await CaseService.GetAsync(Runtime.DbContext, tenantId.Value, regulationId, apiCaseRelation.TargetCaseName);
        if (targetCase != null)
        {
            apiCaseRelation.TargetCaseNameLocalizations = targetCase.NameLocalizations;
            if (!string.IsNullOrWhiteSpace(apiCaseRelation.TargetCaseSlot))
            {
                var slot = targetCase.Slots?.FirstOrDefault(x => string.Equals(x.Name, apiCaseRelation.TargetCaseSlot));
                if (slot != null)
                {
                    apiCaseRelation.TargetCaseSlotLocalizations = slot.NameLocalizations;
                }
            }
        }

        try
        {
            // filter existing relation (OData syntax)
            var query = QueryFactory.NewEqualFilterQuery(new Dictionary<string, object>
            {
                { Persistence.DbSchema.CaseRelationColumn.SourceCaseName, apiCaseRelation.SourceCaseName },
                { Persistence.DbSchema.CaseRelationColumn.SourceCaseSlot, apiCaseRelation.SourceCaseSlot },
                { Persistence.DbSchema.CaseRelationColumn.TargetCaseName, apiCaseRelation.TargetCaseName },
                { Persistence.DbSchema.CaseRelationColumn.TargetCaseSlot, apiCaseRelation.TargetCaseSlot }
            });
            var relations = await Service.QueryAsync(Runtime.DbContext, regulationId, query);
            if (relations.Any())
            {
                return BadRequest($"Case relation from {apiCaseRelation.SourceCaseName}.{apiCaseRelation.SourceCaseSlot} to {apiCaseRelation.TargetCaseName}.{apiCaseRelation.TargetCaseSlot} already exists");
            }

            return await base.CreateAsync(regulationId, apiCaseRelation);
        }
        catch (QueryException exception)
        {
            return QueryError(exception);
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }
}