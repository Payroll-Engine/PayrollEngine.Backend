using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class LookupValueDataMap : ApiMapBase<DomainObject.LookupValueData, ApiObject.LookupValueData>
{
    public override partial ApiObject.LookupValueData ToApi(DomainObject.LookupValueData domainObject);
    public override partial DomainObject.LookupValueData ToDomain(ApiObject.LookupValueData apiObject);
}