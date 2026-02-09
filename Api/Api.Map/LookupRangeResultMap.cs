using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class LookupRangeResultMap : ApiMapBase<DomainObject.LookupRangeResult, ApiObject.LookupRangeResult>
{
    public override partial ApiObject.LookupRangeResult ToApi(DomainObject.LookupRangeResult domainObject);
    public override partial DomainObject.LookupRangeResult ToDomain(ApiObject.LookupRangeResult apiObject);
}
