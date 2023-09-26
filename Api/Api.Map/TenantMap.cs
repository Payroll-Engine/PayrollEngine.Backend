using Riok.Mapperly.Abstractions;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class TenantMap : ApiMapBase<DomainObject.Tenant, ApiObject.Tenant>
{
    public override partial ApiObject.Tenant ToApi(DomainObject.Tenant domainObject);
    public override partial DomainObject.Tenant ToDomain(ApiObject.Tenant apiObject);
}
