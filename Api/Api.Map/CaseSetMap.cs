using AutoMapper;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
public class CaseSetMap : CaseMapBase<DomainObject.CaseSet, ApiObject.CaseSet>
{
    protected override void SetupDomainToApiMapper(IMapperConfigurationExpression configuration)
    {
        base.SetupDomainToApiMapper(configuration);
        configuration.CreateMap<DomainObject.CaseFieldSet, ApiObject.CaseFieldSet>();
        configuration.CreateMap<DomainObject.LookupSettings, ApiObject.LookupSettings>();
    }

    protected override void SetupApiToDomainMapper(IMapperConfigurationExpression configuration)
    {
        base.SetupApiToDomainMapper(configuration);
        configuration.CreateMap<ApiObject.CaseFieldSet, DomainObject.CaseFieldSet>();
        configuration.CreateMap<ApiObject.LookupSettings, DomainObject.LookupSettings>();
    }
}