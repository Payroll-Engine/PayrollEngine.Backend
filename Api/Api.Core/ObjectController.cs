using System;
using System.Collections.Generic;
using System.Linq;
using PayrollEngine.Api.Model;
using PayrollEngine.Domain.Model;
using Microsoft.AspNetCore.Mvc;

namespace PayrollEngine.Api.Core;

public abstract class ObjectController<TDomain, TApi> : ApiController
    where TDomain : class, IDomainObject, new()
    where TApi : ApiObjectBase, new()
{
    protected IApiMap<TDomain, TApi> Map { get; }

    protected string GetObjectName(Type type) => type.Name;
    protected string ObjectName => GetObjectName(typeof(TApi));
    protected string GetObjectName(ApiObjectBase apiObject) =>
        apiObject != null ? GetObjectName(apiObject.GetType()) : null;

    protected ObjectController(IControllerRuntime runtime, IApiMap<TDomain, TApi> map) :
        base(runtime)
    {
        Map = map ?? throw new ArgumentNullException(nameof(map));
    }

    #region Mapping

    protected virtual TApi MapDomainToApi(TDomain domainObject) =>
        Map.ToApi(domainObject);

    protected virtual TApi[] MapDomainToApi(IEnumerable<TDomain> domainObjects) =>
        domainObjects.Select(MapDomainToApi).ToArray();

    protected virtual TDomain MapApiToDomain(TApi apiObject) =>
        Map.ToDomain(apiObject);

    protected virtual IEnumerable<TDomain> MapApiToDomain(TApi[] apiObjects) =>
        apiObjects.Select(MapApiToDomain);

    #endregion

    #region Response results

    protected NotFoundObjectResult NotFound(int id) =>
        NotFound(ObjectName, id);

    protected NotFoundObjectResult NotFound(ApiObjectBase apiObject, int id) =>
        NotFound($"{GetObjectName(apiObject)} with id {id} was not found");

    protected NotFoundObjectResult NotFound(Type type, int id) =>
        NotFound($"{GetObjectName(type)} with id {id} was not found");

    protected NotFoundObjectResult NotFound(string objectName, int id) =>
        NotFound($"{objectName} with id {id} was not found");

    protected BadRequestObjectResult QueryError(QueryException exception) =>
        BadRequest(exception.Message);

    #endregion
}