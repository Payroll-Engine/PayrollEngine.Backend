using PayrollEngine.Api.Core;
using AutoMapper;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
public class CaseAuditMap : ApiMapBase<DomainObject.CaseAudit, ApiObject.CaseAudit>
{
    protected override void SetupDomainToApiMapper(IMapperConfigurationExpression configuration)
    {
        base.SetupDomainToApiMapper(configuration);
        configuration.CreateMap<DomainObject.CaseSlot, ApiObject.CaseSlot>();
        configuration.CreateMap<DomainObject.CaseFieldReference, ApiObject.CaseFieldReference>();
    }

    protected override void SetupApiToDomainMapper(IMapperConfigurationExpression configuration)
    {
        base.SetupApiToDomainMapper(configuration);
        configuration.CreateMap<ApiObject.CaseSlot, DomainObject.CaseSlot>();
        configuration.CreateMap<ApiObject.CaseFieldReference, DomainObject.CaseFieldReference>();
    }
}