using Riok.Mapperly.Abstractions;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class CaseValueMap : ApiMapBase<DomainObject.CaseValue, ApiObject.CaseValue>
{
    public override partial ApiObject.CaseValue ToApi(DomainObject.CaseValue domainObject);
    public override partial DomainObject.CaseValue ToDomain(ApiObject.CaseValue apiObject);
}
