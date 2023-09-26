using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class CaseFieldAuditMap : ApiMapBase<DomainObject.CaseFieldAudit, ApiObject.CaseFieldAudit>
{
    public override partial ApiObject.CaseFieldAudit ToApi(DomainObject.CaseFieldAudit domainObject);
    public override partial DomainObject.CaseFieldAudit ToDomain(ApiObject.CaseFieldAudit apiObject);
}