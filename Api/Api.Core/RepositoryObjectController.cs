using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using PayrollEngine.Api.Model;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application.Service;

namespace PayrollEngine.Api.Core;

public abstract class RepositoryObjectController<TService, TRepo, TDomain, TApi> : ObjectController<TDomain, TApi>
    where TService : class, IRepositoryApplicationService<TRepo>
    where TRepo : class, IDomainRepository
    where TDomain : class, IDomainObject, new()
    where TApi : ApiObjectBase, new()
{
    protected TService Service { get; }

    protected RepositoryObjectController(TService service, IControllerRuntime runtime, IApiMap<TDomain, TApi> map) :
        base(runtime, map)
    {
        Service = service ?? throw new ArgumentNullException(nameof(service));
    }

    /// <summary>Gets the current evaluation date, add some time delay for newly created cases</summary>
    public DateTime CurrentEvaluationDate => Date.Now.AddSeconds(1);

    /// <summary>Test if objects exists</summary>
    /// <param name="context">The database context</param>
    /// <param name="id">The object id</param>
    /// <returns>True if object exists</returns>
    protected virtual async Task<bool> ExistsAsync(IDbContext context, int id) =>
        await Service.ExistsAsync(context, id);

    /// <summary>Creates new culture</summary>
    /// <param name="culture">The culture name</param>
    /// <returns>The culture info, bad request on unknown culture</returns>
    protected CultureInfo NewCultureInfo(string culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            return CultureInfo.CurrentCulture;
        }
        if (!CultureInfo.GetCultures(CultureTypes.AllCultures).Any(
                info => string.Equals(info.Name, culture, StringComparison.CurrentCultureIgnoreCase)))
        {
            return null;
        }
        return new(culture);
    }

    #region Attributes

    protected virtual async Task<ActionResult<bool>> ExistsAttributeAsync(int id, string attributeName) =>
        await Service.ExistsAttributeAsync(Runtime.DbContext, id, attributeName);

    protected virtual async Task<ActionResult<string>> GetAttributeAsync(int id, string attributeName)
    {
        var attribute = await Service.GetAttributeAsync(Runtime.DbContext, id, attributeName);
        if (attribute == null)
        {
            return NotFound();
        }
        return attribute;
    }

    protected virtual async Task<ActionResult<string>> SetAttributeAsync(int id, string attributeName, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return BadRequest($"Missing value for attribute {attributeName}");
        }

        try
        {
            JsonSerializer.Deserialize<object>(value);
        }
        catch
        {
            return BadRequest($"Invalid JSON value for attribute {attributeName}: {value}");
        }

        var attribute = await Service.SetAttributeAsync(Runtime.DbContext, id, attributeName, value);
        if (attribute == null)
        {
            return NotFound();
        }
        return attribute;
    }

    protected virtual async Task<ActionResult<bool>> DeleteAttributeAsync(int id, string attributeName)
    {
        var delete = await Service.DeleteAttributeAsync(Runtime.DbContext, id, attributeName);
        if (delete == null)
        {
            return NotFound();
        }
        return true;
    }

    #endregion

    #region Response results

    protected BadRequestObjectResult UndefinedObjectRequest() =>
        BadRequest($"Invalid {ObjectName}");

    protected BadRequestObjectResult UndefinedObjectIdRequest() =>
        BadRequest($"Invalid id to get object {ObjectName}");

    protected BadRequestObjectResult MissingUpdateObjectIdRequest() =>
        BadRequest($"Missing id to update object {ObjectName}");

    protected BadRequestObjectResult CreateObjectWithIdRequest() =>
        BadRequest($"Id must be zero to create the object {ObjectName}");

    protected BadRequestObjectResult ObjectNotFoundRequest(int id) =>
        BadRequest($"Not found {ObjectName} with id {id}");

    protected BadRequestObjectResult CreateObjectFailedRequest() =>
        BadRequest($"Error while creating the {ObjectName}");

    #endregion
}