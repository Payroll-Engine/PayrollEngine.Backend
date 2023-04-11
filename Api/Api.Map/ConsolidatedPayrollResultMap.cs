using PayrollEngine.Api.Core;
using AutoMapper;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
public class ConsolidatedPayrollResultMap : ApiMapBase<DomainObject.ConsolidatedPayrollResult, ApiObject.ConsolidatedPayrollResult>
{
    protected override void SetupDomainToApiMapper(IMapperConfigurationExpression configuration)
    {
        base.SetupDomainToApiMapper(configuration);
        configuration.CreateMap<DomainObject.WageTypeResultSet, ApiObject.WageTypeResultSet>();
        configuration.CreateMap<DomainObject.CollectorResultSet, ApiObject.CollectorResultSet>();
        configuration.CreateMap<DomainObject.PayrunResult, ApiObject.PayrunResult>();
    }

    protected override void SetupApiToDomainMapper(IMapperConfigurationExpression configuration)
    {
        base.SetupApiToDomainMapper(configuration);
        configuration.CreateMap<ApiObject.WageTypeResultSet, DomainObject.WageTypeResultSet>();
        configuration.CreateMap<ApiObject.CollectorResultSet, DomainObject.CollectorResultSet>();
        configuration.CreateMap<ApiObject.PayrunResult, DomainObject.PayrunResult>();
    }
}