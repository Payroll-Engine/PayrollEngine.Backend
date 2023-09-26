using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class CaseSetMap : ApiMapBase<DomainObject.CaseSet, ApiObject.CaseSet>
{
    public override partial ApiObject.CaseSet ToApi(DomainObject.CaseSet domainObject);
    public override partial DomainObject.CaseSet ToDomain(ApiObject.CaseSet apiObject);
}