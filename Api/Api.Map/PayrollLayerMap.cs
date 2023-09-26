using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class PayrollLayerMap : ApiMapBase<DomainObject.PayrollLayer, ApiObject.PayrollLayer>
{
    public override partial ApiObject.PayrollLayer ToApi(DomainObject.PayrollLayer domainObject);
    public override partial DomainObject.PayrollLayer ToDomain(ApiObject.PayrollLayer apiObject);
}