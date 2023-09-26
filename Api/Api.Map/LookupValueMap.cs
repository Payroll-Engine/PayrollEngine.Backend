using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class LookupValueMap : ApiMapBase<DomainObject.LookupValue, ApiObject.LookupValue>
{
    public override partial ApiObject.LookupValue ToApi(DomainObject.LookupValue domainObject);
    public override partial DomainObject.LookupValue ToDomain(ApiObject.LookupValue apiObject);
}