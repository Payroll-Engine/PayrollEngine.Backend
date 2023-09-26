using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class DivisionMap : ApiMapBase<DomainObject.Division, ApiObject.Division>
{
    public override partial ApiObject.Division ToApi(DomainObject.Division domainObject);
    public override partial DomainObject.Division ToDomain(ApiObject.Division apiObject);
}