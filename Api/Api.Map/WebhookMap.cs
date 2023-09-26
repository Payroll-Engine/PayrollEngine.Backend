using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class WebhookMap : ApiMapBase<DomainObject.Webhook, ApiObject.Webhook>
{
    public override partial ApiObject.Webhook ToApi(DomainObject.Webhook domainObject);
    public override partial DomainObject.Webhook ToDomain(ApiObject.Webhook apiObject);
}