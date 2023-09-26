using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class LookupValueAuditMap : ApiMapBase<DomainObject.LookupValueAudit, ApiObject.LookupValueAudit>
{
    public override partial ApiObject.LookupValueAudit ToApi(DomainObject.LookupValueAudit domainObject);
    public override partial DomainObject.LookupValueAudit ToDomain(ApiObject.LookupValueAudit apiObject);
}