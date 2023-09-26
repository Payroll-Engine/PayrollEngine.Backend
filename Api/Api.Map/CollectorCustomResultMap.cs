using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class CollectorCustomResultMap : ApiMapBase<DomainObject.CollectorCustomResult, ApiObject.CollectorCustomResult>
{
    public override partial ApiObject.CollectorCustomResult ToApi(DomainObject.CollectorCustomResult domainObject);
    public override partial DomainObject.CollectorCustomResult ToDomain(ApiObject.CollectorCustomResult apiObject);
}