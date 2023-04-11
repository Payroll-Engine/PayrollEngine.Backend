using PayrollEngine.Api.Core;
using AutoMapper;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
public class WageTypeResultSetMap : ApiMapBase<DomainObject.WageTypeResultSet, ApiObject.WageTypeResultSet>
{
    protected override void SetupDomainToApiMapper(IMapperConfigurationExpression configuration)
    {
        base.SetupDomainToApiMapper(configuration);
        configuration.CreateMap<DomainObject.WageTypeResult, ApiObject.WageTypeResult>();
        configuration.CreateMap<DomainObject.WageTypeCustomResult, ApiObject.WageTypeCustomResult>();
    }

    protected override void SetupApiToDomainMapper(IMapperConfigurationExpression configuration)
    {
        base.SetupApiToDomainMapper(configuration);
        configuration.CreateMap<ApiObject.WageTypeResult, DomainObject.WageTypeResult>();
        configuration.CreateMap<ApiObject.WageTypeCustomResult, DomainObject.WageTypeCustomResult>();
    }
}