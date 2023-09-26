using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class CollectorResultMap : ApiMapBase<DomainObject.CollectorResult, ApiObject.CollectorResult>
{
    public override partial ApiObject.CollectorResult ToApi(DomainObject.CollectorResult domainObject);
    public override partial DomainObject.CollectorResult ToDomain(ApiObject.CollectorResult apiObject);
}