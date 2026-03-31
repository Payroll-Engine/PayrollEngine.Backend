using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class PayrunJobSetMap : ApiMapBase<DomainObject.PayrunJobSet, ApiObject.PayrunJobSet>
{
    public override partial ApiObject.PayrunJobSet ToApi(DomainObject.PayrunJobSet domainObject);
    public override partial DomainObject.PayrunJobSet ToDomain(ApiObject.PayrunJobSet apiObject);
}
