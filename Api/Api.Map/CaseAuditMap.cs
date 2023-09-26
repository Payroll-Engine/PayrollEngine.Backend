using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class CaseAuditMap : ApiMapBase<DomainObject.CaseAudit, ApiObject.CaseAudit>
{
    public override partial ApiObject.CaseAudit ToApi(DomainObject.CaseAudit domainObject);
    public override partial DomainObject.CaseAudit ToDomain(ApiObject.CaseAudit apiObject);
}