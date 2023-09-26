using System.Collections.Generic;
using PayrollEngine.Api.Core;

namespace PayrollEngine.Api.Map;

public abstract class ApiMapBase<TDomain, TApi> : IApiMap<TDomain, TApi>
    where TDomain : class, new()
    where TApi : class, new()
{

    #region Doamin to Api

    public abstract TApi ToApi(TDomain domainObject);

    public TApi[] ToApi(IEnumerable<TDomain> domainObject)
    {
        var targets = new List<TApi>();
        if (domainObject != null)
        {
            foreach (var source in domainObject)
            {
                targets.Add(ToApi(source));
            }
        }
        return targets.ToArray();
    }

    #endregion

    #region API to Domain

    public abstract TDomain ToDomain(TApi apiObject);

    public IEnumerable<TDomain> ToDomain(IEnumerable<TApi> apiObjects)
    {
        var domainObjects = new List<TDomain>();
        if (apiObjects != null)
        {
            foreach (var apiObject in apiObjects)
            {
                domainObjects.Add(ToDomain(apiObject));
            }
        }
        return domainObjects;
    }

    #endregion
}