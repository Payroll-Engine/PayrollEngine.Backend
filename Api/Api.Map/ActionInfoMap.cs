using PayrollEngine.Api.Core;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using AutoMapper;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
public class ActionInfoMap : ApiMapBase<DomainObject.ActionInfo, ApiObject.ActionInfo>
{
    protected override void SetupDomainToApiMapper(IMapperConfigurationExpression configuration)
    {
        base.SetupDomainToApiMapper(configuration);
        configuration.CreateMap<DomainObject.ActionParameterInfo, ApiObject.ActionParameterInfo>();
        configuration.CreateMap<DomainObject.ActionIssueInfo, ApiObject.ActionIssueInfo>();
    }

    protected override void SetupApiToDomainMapper(IMapperConfigurationExpression configuration)
    {
        base.SetupApiToDomainMapper(configuration);
        configuration.CreateMap<ApiObject.ActionParameterInfo, DomainObject.ActionParameterInfo>();
        configuration.CreateMap<ApiObject.ActionIssueInfo, DomainObject.ActionIssueInfo>();
    }
}