using PayrollEngine.Api.Core;
using AutoMapper;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
public class CaseChangeSetupMap : ApiMapBase<DomainObject.CaseChangeSetup, ApiObject.CaseChangeSetup>
{
    protected override void SetupDomainToApiMapper(IMapperConfigurationExpression configuration)
    {
        base.SetupDomainToApiMapper(configuration);
        configuration.CreateMap<DomainObject.CaseSetup, ApiObject.CaseSetup>();
        configuration.CreateMap<DomainObject.CaseValueSetup, ApiObject.CaseValueSetup>();
        configuration.CreateMap<DomainObject.CaseDocument, ApiObject.CaseDocument>();
        configuration.CreateMap<DomainObject.CaseFieldReference, ApiObject.CaseFieldReference>();
        configuration.CreateMap<DomainObject.CaseValidationIssue, ApiObject.CaseValidationIssue>();
    }

    protected override void SetupApiToDomainMapper(IMapperConfigurationExpression configuration)
    {
        base.SetupApiToDomainMapper(configuration);
        configuration.CreateMap<ApiObject.CaseSetup, DomainObject.CaseSetup>();
        configuration.CreateMap<ApiObject.CaseValueSetup, DomainObject.CaseValueSetup>();
        configuration.CreateMap<ApiObject.CaseDocument, DomainObject.CaseDocument>();
        configuration.CreateMap<ApiObject.CaseFieldReference, DomainObject.CaseFieldReference>();
        configuration.CreateMap<ApiObject.CaseValidationIssue, DomainObject.CaseValidationIssue>();
    }
}