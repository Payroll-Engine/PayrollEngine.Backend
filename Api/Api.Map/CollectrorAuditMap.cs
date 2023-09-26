using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class CollectorAuditMap : ApiMapBase<DomainObject.CollectorAudit, ApiObject.CollectorAudit>
{
    public override partial ApiObject.CollectorAudit ToApi(DomainObject.CollectorAudit domainObject);
    public override partial DomainObject.CollectorAudit ToDomain(ApiObject.CollectorAudit apiObject);
}