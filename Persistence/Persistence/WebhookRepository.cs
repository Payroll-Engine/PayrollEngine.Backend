using System.Data;
using PayrollEngine.Domain.Model;
using PayrollEngine.Serialization;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class WebhookRepository() : ChildDomainRepository<Webhook>(DbSchema.Tables.Webhook,
    DbSchema.WebhookColumn.TenantId), IWebhookRepository
{
    protected override void GetObjectCreateData(Webhook webhook, DbParameterCollection parameters)
    {
        parameters.Add(nameof(webhook.Name), webhook.Name);
        base.GetObjectCreateData(webhook, parameters);
    }

    protected override void GetObjectData(Webhook webhook, DbParameterCollection parameters)
    {
        parameters.Add(nameof(webhook.ReceiverAddress), webhook.ReceiverAddress);
        parameters.Add(nameof(webhook.Action), webhook.Action, DbType.Int32);
        parameters.Add(nameof(webhook.Attributes), JsonSerializer.SerializeNamedDictionary(webhook.Attributes));
        base.GetObjectData(webhook, parameters);
    }
}