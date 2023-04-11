using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
public class CaseChangeCaseValueMap : CaseValueMapBase<DomainObject.CaseChangeCaseValue, ApiObject.CaseChangeCaseValue>
{
}