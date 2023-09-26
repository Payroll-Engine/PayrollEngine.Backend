using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class LookupDataMap : ApiMapBase<DomainObject.LookupData, ApiObject.LookupData>
{
    public override partial ApiObject.LookupData ToApi(DomainObject.LookupData domainObject);
    public override partial DomainObject.LookupData ToDomain(ApiObject.LookupData apiObject);
}