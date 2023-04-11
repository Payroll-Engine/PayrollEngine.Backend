using System.Collections.Generic;

namespace PayrollEngine.Api.Core;

public interface IApiMap<TDomain, TApi>
    where TDomain : class, new()
    where TApi : class, new()
{
    TApi ToApi(TDomain domainObject);
    TApi ToApi(TDomain domainObject, TApi apiObject);
    IEnumerable<TDomain> ToDomain(IEnumerable<TApi> apiObjects);

    TDomain ToDomain(TApi apiObject);
    TDomain ToDomain(TApi apiObject, TDomain domainObject);
    TApi[] ToApi(IEnumerable<TDomain> domainObject);
}