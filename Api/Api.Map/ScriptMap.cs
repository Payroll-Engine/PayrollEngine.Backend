using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class ScriptMap : ApiMapBase<DomainObject.Script, ApiObject.Script>
{
    public override partial ApiObject.Script ToApi(DomainObject.Script domainObject);
    public override partial DomainObject.Script ToDomain(ApiObject.Script apiObject);
}