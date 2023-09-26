using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class ReportParameterAuditMap : ApiMapBase<DomainObject.ReportParameterAudit, ApiObject.ReportParameterAudit>
{
    public override partial ApiObject.ReportParameterAudit ToApi(DomainObject.ReportParameterAudit domainObject);
    public override partial DomainObject.ReportParameterAudit ToDomain(ApiObject.ReportParameterAudit apiObject);
}