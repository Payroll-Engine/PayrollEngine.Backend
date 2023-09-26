using Riok.Mapperly.Abstractions;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class CaseMap : ApiMapBase<DomainObject.Case, ApiObject.Case>
{
    public override partial ApiObject.Case ToApi(DomainObject.Case domainObject);
    public override partial DomainObject.Case ToDomain(ApiObject.Case apiObject);
}
