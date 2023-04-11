using PayrollEngine.Api.Core;
using AutoMapper;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
public class CollectorResultSetMap : ApiMapBase<DomainObject.CollectorResultSet, ApiObject.CollectorResultSet>
{
    protected override void SetupDomainToApiMapper(IMapperConfigurationExpression configuration)
    {
        base.SetupDomainToApiMapper(configuration);
        configuration.CreateMap<DomainObject.CollectorResult, ApiObject.CollectorResult>();
        configuration.CreateMap<DomainObject.CollectorCustomResult, ApiObject.CollectorCustomResult>();
    }

    protected override void SetupApiToDomainMapper(IMapperConfigurationExpression configuration)
    {
        base.SetupApiToDomainMapper(configuration);
        configuration.CreateMap<ApiObject.CollectorResult, DomainObject.CollectorResult>();
        configuration.CreateMap<ApiObject.CollectorCustomResult, DomainObject.CollectorCustomResult>();
    }
}