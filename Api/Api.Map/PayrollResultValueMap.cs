using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class PayrollResultValueMap : ApiMapBase<DomainObject.PayrollResultValue, ApiObject.PayrollResultValue>
{
    public override partial ApiObject.PayrollResultValue ToApi(DomainObject.PayrollResultValue domainObject);
    public override partial DomainObject.PayrollResultValue ToDomain(ApiObject.PayrollResultValue apiObject);
}