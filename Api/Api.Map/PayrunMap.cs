using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class PayrunMap : ApiMapBase<DomainObject.Payrun, ApiObject.Payrun>
{
    public override partial ApiObject.Payrun ToApi(DomainObject.Payrun domainObject);
    public override partial DomainObject.Payrun ToDomain(ApiObject.Payrun apiObject);
}