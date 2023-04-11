using PayrollEngine.Api.Core;
using AutoMapper;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
public class CaseSetupMap : ApiMapBase<DomainObject.CaseSetup, ApiObject.CaseSetup>
{
    protected override void SetupDomainToApiMapper(IMapperConfigurationExpression configuration)
    {
        base.SetupDomainToApiMapper(configuration);
        configuration.CreateMap<DomainObject.CaseValueSetup, ApiObject.CaseValueSetup>();
        configuration.CreateMap<DomainObject.CaseDocument, ApiObject.CaseDocument>();
    }

    protected override void SetupApiToDomainMapper(IMapperConfigurationExpression configuration)
    {
        base.SetupApiToDomainMapper(configuration);
        configuration.CreateMap<ApiObject.CaseValueSetup, DomainObject.CaseValueSetup>();
        configuration.CreateMap<ApiObject.CaseDocument, DomainObject.CaseDocument>();
    }
}