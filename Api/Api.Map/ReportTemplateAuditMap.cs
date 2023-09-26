using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class ReportTemplateAuditMap : ApiMapBase<DomainObject.ReportTemplateAudit, ApiObject.ReportTemplateAudit>
{
    public override partial ApiObject.ReportTemplateAudit ToApi(DomainObject.ReportTemplateAudit domainObject);
    public override partial DomainObject.ReportTemplateAudit ToDomain(ApiObject.ReportTemplateAudit apiObject);
}