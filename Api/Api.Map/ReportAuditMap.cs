using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class ReportAuditMap : ApiMapBase<DomainObject.ReportAudit, ApiObject.ReportAudit>
{
    public override partial ApiObject.ReportAudit ToApi(DomainObject.ReportAudit domainObject);
    public override partial DomainObject.ReportAudit ToDomain(ApiObject.ReportAudit apiObject);
}
