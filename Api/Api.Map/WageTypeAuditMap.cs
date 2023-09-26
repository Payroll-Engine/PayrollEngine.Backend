using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class WageTypeAuditMap : ApiMapBase<DomainObject.WageTypeAudit, ApiObject.WageTypeAudit>
{
    public override partial ApiObject.WageTypeAudit ToApi(DomainObject.WageTypeAudit domainObject);
    public override partial DomainObject.WageTypeAudit ToDomain(ApiObject.WageTypeAudit apiObject);
}