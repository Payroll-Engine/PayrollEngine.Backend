using PayrollEngine.Api.Core;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
public class CaseValidationIssueMap : ApiMapBase<DomainObject.CaseValidationIssue, ApiObject.CaseValidationIssue>
{
}