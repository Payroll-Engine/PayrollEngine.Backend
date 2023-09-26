using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class PayrunResultMap : ApiMapBase<DomainObject.PayrunResult, ApiObject.PayrunResult>
{
    public override partial ApiObject.PayrunResult ToApi(DomainObject.PayrunResult domainObject);
    public override partial DomainObject.PayrunResult ToDomain(ApiObject.PayrunResult apiObject);
}