using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class LogMap : ApiMapBase<DomainObject.Log, ApiObject.Log>
{
    public override partial ApiObject.Log ToApi(DomainObject.Log domainObject);
    public override partial DomainObject.Log ToDomain(ApiObject.Log apiObject);
}