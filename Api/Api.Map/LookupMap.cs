using PayrollEngine.Api.Core;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
public class LookupMap : LookupMap<DomainObject.Lookup, ApiObject.Lookup>
{
}

/// <summary>
/// Map a domain object with an api object
/// </summary>
public class LookupMap<TDomain, TApi> : ApiMapBase<TDomain, TApi>
    where TDomain : DomainObject.Lookup, new()
    where TApi : ApiObject.Lookup, new()
{
}