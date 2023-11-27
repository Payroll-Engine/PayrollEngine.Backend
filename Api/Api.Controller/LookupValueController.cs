using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Domain.Model.Repository;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application.Service;
using ApiObject = PayrollEngine.Api.Model;
using PayrollEngine.Domain.Model;
// ReSharper disable UnusedParameter.Global

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for the regulation lookup values
/// </summary>
public abstract class LookupValueController(ILookupService lookupService, ILookupValueService lookupValueService,
        IControllerRuntime runtime)
    : RepositoryChildObjectController<ILookupService, ILookupValueService,
    ILookupRepository, ILookupValueRepository,
    Lookup, LookupValue, ApiObject.LookupValue>(lookupService, lookupValueService, runtime, new LookupValueMap())
{
    public virtual async Task<ActionResult<ApiObject.LookupValueData[]>> GetLookupValuesDataAsync(
        int tenantId, int regulationId, int lookupId, string culture)
    {
        var lookupValues = (await QueryAsync(lookupId)).Value;
        if (lookupValues == null)
        {
            return NotFound();
        }

        var result = lookupValues.Select(x => new ApiObject.LookupValueData
        {
            Key = x.Key,
            Value = !string.IsNullOrWhiteSpace(culture) ?
                culture.GetLocalization(x.ValueLocalizations, x.Value) :
                x.Value,
            RangeValue = x.RangeValue
        });
        return result.ToArray();
    }

    protected override async Task<ActionResult<ApiObject.LookupValue>> CreateAsync(
        int lookupId, ApiObject.LookupValue lookupValue)
    {
        if (string.IsNullOrWhiteSpace(lookupValue.Key))
        {
            return BadRequest("Lookup value without key");
        }
        // unique lookup key per lookup
        if (await ChildService.ExistsAsync(Runtime.DbContext, lookupId, lookupValue.Key, lookupValue.RangeValue))
        {
            return lookupValue.RangeValue.HasValue ?
                BadRequest($"Lookup value with key {lookupValue.Key} and range value {lookupValue.RangeValue} already exists") :
                BadRequest($"Lookup value with key {lookupValue.Key} already exists");
        }

        return await base.CreateAsync(lookupId, lookupValue);
    }
}