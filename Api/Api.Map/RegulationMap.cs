using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class RegulationMap : ApiMapBase<DomainObject.Regulation, ApiObject.Regulation>
{
    public override partial ApiObject.Regulation ToApi(DomainObject.Regulation domainObject);
    public override partial DomainObject.Regulation ToDomain(ApiObject.Regulation apiObject);
}