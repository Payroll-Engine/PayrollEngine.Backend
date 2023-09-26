using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class CollectorMap : ApiMapBase<DomainObject.Collector, ApiObject.Collector>
{
    public override partial ApiObject.Collector ToApi(DomainObject.Collector domainObject);
    public override partial DomainObject.Collector ToDomain(ApiObject.Collector apiObject);
}