using AutoMapper;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
public class CaseValueSetupMapBase<TDomain, TApi> : CaseValueMapBase<TDomain, TApi>
    where TDomain : DomainObject.CaseValueSetup, new()
    where TApi : ApiObject.CaseValueSetup, new()
{
    protected override void SetupDomainToApiMapper(IMapperConfigurationExpression configuration)
    {
        base.SetupDomainToApiMapper(configuration);
        configuration.CreateMap<DomainObject.CaseDocument, ApiObject.CaseDocument>();
        configuration.CreateMap<DomainObject.CaseRelationReference, ApiObject.CaseRelationReference>();
    }

    protected override void SetupApiToDomainMapper(IMapperConfigurationExpression configuration)
    {
        base.SetupApiToDomainMapper(configuration);
        configuration.CreateMap<ApiObject.CaseDocument, DomainObject.CaseDocument>();
        configuration.CreateMap<ApiObject.CaseRelationReference, DomainObject.CaseRelationReference>();
    }
}