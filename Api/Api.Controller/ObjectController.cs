using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using PayrollEngine.Api.Model;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Api.Controller;

public abstract class ObjectController<TDomain, TApi>
    (IControllerRuntime runtime, IApiMap<TDomain, TApi> map) : ApiController(runtime)
    where TDomain : class, IDomainObject, new()
    where TApi : ApiObjectBase, new()
{
    protected IApiMap<TDomain, TApi> Map { get; } = map ?? throw new ArgumentNullException(nameof(map));

    protected string GetObjectName(Type type) => type.Name;
    protected string ObjectName => GetObjectName(typeof(TApi));
    private string GetObjectName(ApiObjectBase apiObject) =>
        apiObject != null ? GetObjectName(apiObject.GetType()) : null;

    #region Mapping

    protected TApi MapDomainToApi(TDomain domainObject) =>
        Map.ToApi(domainObject);

    protected TApi[] MapDomainToApi(IEnumerable<TDomain> domainObjects) =>
        domainObjects.Select(MapDomainToApi).ToArray();

    protected TDomain MapApiToDomain(TApi apiObject) =>
        Map.ToDomain(apiObject);

    protected IEnumerable<TDomain> MapApiToDomain(TApi[] apiObjects) =>
        apiObjects.Select(MapApiToDomain);

    #endregion

    #region Response results

    protected NotFoundObjectResult NotFound(int id) =>
        NotFound(ObjectName, id);

    protected NotFoundObjectResult NotFound(ApiObjectBase apiObject, int id) =>
        NotFound($"{GetObjectName(apiObject)} with id {id} was not found");

    protected NotFoundObjectResult NotFound(Type type, int id) =>
        NotFound($"{GetObjectName(type)} with id {id} was not found");

    private NotFoundObjectResult NotFound(string objectName, int id) =>
        NotFound($"{objectName} with id {id} was not found");

    protected BadRequestObjectResult QueryError(QueryException exception) =>
        BadRequest(exception.Message);

    #endregion
}