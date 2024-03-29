﻿using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;

namespace PayrollEngine.Persistence;

public class WebhookRepository() : ChildDomainRepository<Webhook>(DbSchema.Tables.Webhook,
    DbSchema.WebhookColumn.TenantId), IWebhookRepository
{
    protected override void GetObjectCreateData(Webhook webhook, DbParameterCollection parameters)
    {
        parameters.Add(nameof(webhook.Name), webhook.Name);
        parameters.Add(nameof(webhook.ReceiverAddress), webhook.ReceiverAddress);
        parameters.Add(nameof(webhook.Action), webhook.Action);
        parameters.Add(nameof(webhook.Attributes), JsonSerializer.SerializeNamedDictionary(webhook.Attributes));
        base.GetObjectCreateData(webhook, parameters);
    }
}