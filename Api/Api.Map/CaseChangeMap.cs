using PayrollEngine.Api.Core;
using AutoMapper;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
public class CaseChangeMap<TDomain, TApi> : ApiMapBase<TDomain, TApi>
    where TDomain : DomainObject.CaseChange, new()
    where TApi : ApiObject.CaseChange, new()
{
    protected override void SetupDomainToApiMapper(IMapperConfigurationExpression configuration)
    {
        base.SetupDomainToApiMapper(configuration);
        configuration.CreateMap<DomainObject.CaseValue, ApiObject.CaseValue>();
        configuration.CreateMap<DomainObject.CaseValueSetup, ApiObject.CaseValueSetup>();
        configuration.CreateMap<DomainObject.CaseRelationReference, ApiObject.CaseRelationReference>();
    }

    protected override void SetupApiToDomainMapper(IMapperConfigurationExpression configuration)
    {
        base.SetupApiToDomainMapper(configuration);
        configuration.CreateMap<ApiObject.CaseValue, DomainObject.CaseValue>();
        configuration.CreateMap<ApiObject.CaseValueSetup, DomainObject.CaseValueSetup>();
        configuration.CreateMap<ApiObject.CaseRelationReference, DomainObject.CaseRelationReference>();
    }
}