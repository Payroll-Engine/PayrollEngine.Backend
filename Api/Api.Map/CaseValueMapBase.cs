using PayrollEngine.Api.Core;
using AutoMapper;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
public class CaseValueMapBase<TDomain, TApi> : ApiMapBase<TDomain, TApi>
    where TDomain : DomainObject.CaseValue, new()
    where TApi : ApiObject.CaseValue, new()
{
    protected override void SetupDomainToApiMapper(IMapperConfigurationExpression configuration)
    {
        base.SetupDomainToApiMapper(configuration);
        configuration.CreateMap<DomainObject.CaseRelationReference, ApiObject.CaseRelationReference>();
    }

    protected override void SetupApiToDomainMapper(IMapperConfigurationExpression configuration)
    {
        base.SetupApiToDomainMapper(configuration);
        configuration.CreateMap<ApiObject.CaseRelationReference, DomainObject.CaseRelationReference>();
    }
}