using System;
using System.Collections.Generic;
using AutoMapper;

namespace PayrollEngine.Api.Core;

public abstract class ApiMapBase<TDomain, TApi> : IApiMap<TDomain, TApi>
    where TDomain : class, new()
    where TApi : class, new()
{

    #region Doamin to Api

    public TApi ToApi(TDomain domainObject) => ToApi(domainObject, new());
    public TApi ToApi(TDomain domainObject, TApi apiObject)
    {
        if (apiObject == null)
        {
            throw new ArgumentNullException(nameof(apiObject));
        }
        if (domainObject != null)
        {
            MapDomainToApi(domainObject, apiObject);
        }
        return apiObject;
    }

    public TApi[] ToApi(IEnumerable<TDomain> domainObject)
    {
        var targets = new List<TApi>();
        if (domainObject != null)
        {
            foreach (var source in domainObject)
            {
                targets.Add(ToApi(source));
            }
        }
        return targets.ToArray();
    }

    protected virtual void MapDomainToApi(TDomain domainObject, TApi apiObject)
    {
        try
        {
            var config = new MapperConfiguration(SetupDomainToApiMapper);
            new Mapper(config).Map(domainObject, apiObject);
        }
        catch (Exception exception)
        {
            throw new PayrollException($"Mapping error in object {apiObject}: {exception.GetBaseMessage()}", exception);
        }
    }

    protected virtual void SetupDomainToApiMapper(IMapperConfigurationExpression configuration)
    {
        configuration.CreateMap<TDomain, TApi>();
    }

    #endregion

    #region API to Domain

    public TDomain ToDomain(TApi apiObject) => ToDomain(apiObject, new());
    public TDomain ToDomain(TApi apiObject, TDomain domainObject)
    {
        if (domainObject == null)
        {
            throw new ArgumentNullException(nameof(domainObject));
        }
        if (apiObject != null)
        {
            MapApiToDomain(apiObject, domainObject);
        }
        return domainObject;
    }

    public IEnumerable<TDomain> ToDomain(IEnumerable<TApi> apiObjects)
    {
        var domainObjects = new List<TDomain>();
        if (apiObjects != null)
        {
            foreach (var apiObject in apiObjects)
            {
                domainObjects.Add(ToDomain(apiObject));
            }
        }
        return domainObjects;
    }

    protected virtual void MapApiToDomain(TApi apiObject, TDomain domainObject)
    {
        try
        {
            var config = new MapperConfiguration(SetupApiToDomainMapper);
            new Mapper(config).Map(apiObject, domainObject);
        }
        catch (Exception exception)
        {
            throw new PayrollException($"Mapping error in object {apiObject}: {exception.GetBaseMessage()}", exception);
        }

    }
    protected virtual void SetupApiToDomainMapper(IMapperConfigurationExpression configuration)
    {
        configuration.CreateMap<TApi, TDomain>();
    }

    #endregion
}