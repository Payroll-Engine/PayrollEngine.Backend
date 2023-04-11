using PayrollEngine.Api.Core;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
public class ReportAuditMap : ReportAuditMap<DomainObject.ReportAudit, ApiObject.ReportAudit>
{
}

/// <summary>
/// Map a domain object with an api object
/// </summary>
public class ReportAuditMap<TDomain, TApi> : ApiMapBase<TDomain, TApi>
    where TDomain : DomainObject.ReportAudit, new()
    where TApi : ApiObject.ReportAudit, new()
{
}