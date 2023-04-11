using PayrollEngine.Api.Core;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
public class LookupAuditMap : ApiMapBase<DomainObject.LookupAudit, ApiObject.LookupAudit>
{
}