using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class WageTypeResultMap : ApiMapBase<DomainObject.WageTypeResult, ApiObject.WageTypeResult>
{
    public override partial ApiObject.WageTypeResult ToApi(DomainObject.WageTypeResult domainObject);
    public override partial DomainObject.WageTypeResult ToDomain(ApiObject.WageTypeResult apiObject);
}