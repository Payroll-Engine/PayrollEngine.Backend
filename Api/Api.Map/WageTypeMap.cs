using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class WageTypeMap : ApiMapBase<DomainObject.WageType, ApiObject.WageType>
{
    public override partial ApiObject.WageType ToApi(DomainObject.WageType domainObject);
    public override partial DomainObject.WageType ToDomain(ApiObject.WageType apiObject);
}