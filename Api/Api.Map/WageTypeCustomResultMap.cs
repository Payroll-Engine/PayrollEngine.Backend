using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class WageTypeCustomResultMap : ApiMapBase<DomainObject.WageTypeCustomResult, ApiObject.WageTypeCustomResult>
{
    public override partial ApiObject.WageTypeCustomResult ToApi(DomainObject.WageTypeCustomResult domainObject);
    public override partial DomainObject.WageTypeCustomResult ToDomain(ApiObject.WageTypeCustomResult apiObject);
}