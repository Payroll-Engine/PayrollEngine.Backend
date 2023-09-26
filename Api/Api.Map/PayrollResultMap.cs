using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class PayrollResultMap : ApiMapBase<DomainObject.PayrollResult, ApiObject.PayrollResult>
{
    public override partial ApiObject.PayrollResult ToApi(DomainObject.PayrollResult domainObject);
    public override partial DomainObject.PayrollResult ToDomain(ApiObject.PayrollResult apiObject);
}