using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
public class CaseChangeMap<TDomain, TApi> : ApiMapBase<TDomain, TApi>
    where TDomain : DomainObject.CaseChange, new()
    where TApi : ApiObject.CaseChange, new()
{
    private readonly CaseChangeMap map = new();

    public override TApi ToApi(TDomain domainObject) =>
        (TApi)map.ToApi(domainObject);

    public override TDomain ToDomain(TApi apiObject) =>
        (TDomain)map.ToDomain(apiObject);
}
