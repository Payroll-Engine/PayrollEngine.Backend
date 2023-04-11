﻿using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the regulation lookup values
/// </summary>
[ApiControllerName("Lookup values")]
[Route("api/tenants/{tenantId}/regulations/{regulationId}/lookups/{lookupId}/values")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.LookupValue)]
public abstract class LookupValueController : RepositoryChildObjectController<ILookupService, ILookupValueService,
    ILookupRepository, ILookupValueRepository,
    DomainObject.Lookup, DomainObject.LookupValue, ApiObject.LookupValue>
{
    protected LookupValueController(ILookupService lookupService, ILookupValueService lookupValueService, IControllerRuntime runtime) :
        base(lookupService, lookupValueService, runtime, new LookupValueMap())
    {
    }

    public virtual async Task<ActionResult<ApiObject.LookupValueData[]>> GetLookupValuesDataAsync(int tenantId,
        int regulationId, int lookupId, Language? language)
    {
        var lookupValues = (await QueryAsync(lookupId)).Value;
        if (lookupValues == null)
        {
            return NotFound();
        }

        var result = lookupValues.Select(x => new ApiObject.LookupValueData
        {
            Key = x.Key,
            Value = language.HasValue ?
                language.Value.GetLocalization(x.ValueLocalizations, x.Value) :
                x.Value,
            RangeValue = x.RangeValue
        });
        return result.ToArray();
    }

    protected override async Task<ActionResult<ApiObject.LookupValue>> CreateAsync(int lookupId, ApiObject.LookupValue lookupValue)
    {
        if (string.IsNullOrWhiteSpace(lookupValue.Key))
        {
            return BadRequest("Lookup value without key");
        }
        // unique lookup key per lookup
        if (await ChildService.ExistsAsync(lookupId, lookupValue.Key, lookupValue.RangeValue))
        {
            return lookupValue.RangeValue.HasValue ?
                BadRequest($"Lookup value with key {lookupValue.Key} and range value {lookupValue.RangeValue} already exists") :
                BadRequest($"Lookup value with key {lookupValue.Key} already exists");
        }

        return await base.CreateAsync(lookupId, lookupValue);
    }
}