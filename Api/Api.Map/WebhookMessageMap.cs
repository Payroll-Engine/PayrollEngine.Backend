using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;
using Riok.Mapperly.Abstractions;

namespace PayrollEngine.Api.Map;

/// <summary>
/// Map a domain object with an api object
/// </summary>
[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true)]
public partial class WebhookMessageMap : ApiMapBase<DomainObject.WebhookMessage, ApiObject.WebhookMessage>
{
    public override partial ApiObject.WebhookMessage ToApi(DomainObject.WebhookMessage domainObject);
    public override partial DomainObject.WebhookMessage ToDomain(ApiObject.WebhookMessage apiObject);
}