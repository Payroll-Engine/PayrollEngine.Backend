using AutoMapper;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
public class ReportSetMap : ReportMap<DomainObject.ReportSet, ApiObject.ReportSet>
{
    protected override void SetupDomainToApiMapper(IMapperConfigurationExpression configuration)
    {
        base.SetupDomainToApiMapper(configuration);
        configuration.CreateMap<DomainObject.ReportParameter, ApiObject.ReportParameter>();
        configuration.CreateMap<DomainObject.ReportTemplate, ApiObject.ReportTemplate>();
    }

    protected override void SetupApiToDomainMapper(IMapperConfigurationExpression configuration)
    {
        base.SetupApiToDomainMapper(configuration);
        configuration.CreateMap<ApiObject.ReportParameter, DomainObject.ReportParameter>();
        configuration.CreateMap<ApiObject.ReportTemplate, DomainObject.ReportTemplate>();
    }
}