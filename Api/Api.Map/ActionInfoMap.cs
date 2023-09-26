using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class ActionInfoMap : ApiMapBase<DomainObject.ActionInfo, ApiObject.ActionInfo>
{
    public override partial ApiObject.ActionInfo ToApi(DomainObject.ActionInfo domainObject);
    public override partial DomainObject.ActionInfo ToDomain(ApiObject.ActionInfo apiObject);
}