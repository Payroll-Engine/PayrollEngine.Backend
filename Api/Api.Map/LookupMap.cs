using Riok.Mapperly.Abstractions;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class LookupMap : ApiMapBase<DomainObject.Lookup, ApiObject.Lookup>
{
    public override partial ApiObject.Lookup ToApi(DomainObject.Lookup domainObject);
    public override partial DomainObject.Lookup ToDomain(ApiObject.Lookup apiObject);
}
