using System;
using System.Data;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Persistence.DbSchema;

namespace PayrollEngine.Persistence;

/// <summary>Repository for <see cref="WebhookMessage"/> persistence (table: WebhookMessage).</summary>
public class WebhookMessageRepository() : ChildDomainRepository<WebhookMessage>(Tables.WebhookMessage,
    WebhookMessageColumn.WebhookId), IWebhookMessageRepository
{
    protected override void GetObjectCreateData(WebhookMessage message, DbParameterCollection parameters)
    {
        parameters.Add(nameof(message.ActionName), message.ActionName);
        parameters.Add(nameof(message.ReceiverAddress), message.ReceiverAddress);
        parameters.Add(nameof(message.RequestDate), message.RequestDate, DbType.DateTime2);
        parameters.Add(nameof(message.RequestMessage), message.RequestMessage);
        parameters.Add(nameof(message.RequestOperation), message.RequestOperation);
        base.GetObjectCreateData(message, parameters);
    }

    // response data is written on create and on update (the response is received after the message is created)
    protected override void GetObjectData(WebhookMessage message, DbParameterCollection parameters)
    {
        // no response yet: store null instead of the default date
        DateTime? responseDate = message.ResponseDate == default ? null : message.ResponseDate;
        parameters.Add(nameof(message.ResponseDate), responseDate, DbType.DateTime2);
        parameters.Add(nameof(message.ResponseStatus), message.ResponseStatus, DbType.Int32);
        parameters.Add(nameof(message.ResponseMessage), message.ResponseMessage);
        base.GetObjectData(message, parameters);
    }
}