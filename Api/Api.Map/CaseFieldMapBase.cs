using PayrollEngine.Api.Core;
using AutoMapper;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object (derived base)
/// </summary>
public abstract class CaseFieldMapBase<TDomain, TApi> : ApiMapBase<TDomain, TApi>
    where TDomain : DomainObject.CaseField, new()
    where TApi : ApiObject.CaseField, new()
{
    protected override void SetupDomainToApiMapper(IMapperConfigurationExpression configuration)
    {
        base.SetupDomainToApiMapper(configuration);
        configuration.CreateMap<DomainObject.LookupSettings, ApiObject.LookupSettings>();
    }

    protected override void SetupApiToDomainMapper(IMapperConfigurationExpression configuration)
    {
        base.SetupApiToDomainMapper(configuration);
        configuration.CreateMap<ApiObject.LookupSettings, DomainObject.LookupSettings>();
    }
}