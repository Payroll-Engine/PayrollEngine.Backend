using PayrollEngine.Api.Core;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
public class ReportMap : ReportMap<DomainObject.Report, ApiObject.Report>
{
}

/// <summary>
/// Map a domain object with an api object
/// </summary>
public class ReportMap<TDomain, TApi> : ApiMapBase<TDomain, TApi>
    where TDomain : DomainObject.Report, new()
    where TApi : ApiObject.Report, new()
{
}