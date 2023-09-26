using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class RegulationShareMap : ApiMapBase<DomainObject.RegulationShare, ApiObject.RegulationShare>
{
    public override partial ApiObject.RegulationShare ToApi(DomainObject.RegulationShare domainObject);
    public override partial DomainObject.RegulationShare ToDomain(ApiObject.RegulationShare apiObject);
}