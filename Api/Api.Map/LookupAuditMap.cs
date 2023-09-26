using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class LookupAuditMap : ApiMapBase<DomainObject.LookupAudit, ApiObject.LookupAudit>
{
    public override partial ApiObject.LookupAudit ToApi(DomainObject.LookupAudit domainObject);
    public override partial DomainObject.LookupAudit ToDomain(ApiObject.LookupAudit apiObject);
}