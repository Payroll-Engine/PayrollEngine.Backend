using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
// ReSharper disable once UnusedType.Global
public partial class LookupRangeBracketMap : ApiMapBase<DomainObject.LookupRangeBracket, ApiObject.LookupRangeBracket>
{
    public override partial ApiObject.LookupRangeBracket ToApi(DomainObject.LookupRangeBracket domainObject);
    public override partial DomainObject.LookupRangeBracket ToDomain(ApiObject.LookupRangeBracket apiObject);
}
