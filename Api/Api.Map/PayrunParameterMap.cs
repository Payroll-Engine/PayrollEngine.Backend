using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class PayrunParameterMap : ApiMapBase<DomainObject.PayrunParameter, ApiObject.PayrunParameter>
{
    public override partial ApiObject.PayrunParameter ToApi(DomainObject.PayrunParameter domainObject);
    public override partial DomainObject.PayrunParameter ToDomain(ApiObject.PayrunParameter apiObject);
}