using System.Data;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class WebhookMessageRepository() : ChildDomainRepository<WebhookMessage>(DbSchema.Tables.WebhookMessage,
    DbSchema.WebhookMessageColumn.WebhookId), IWebhookMessageRepository
{
    protected override void GetObjectCreateData(WebhookMessage message, DbParameterCollection parameters)
    {
        parameters.Add(nameof(message.ActionName), message.ActionName);
        parameters.Add(nameof(message.ReceiverAddress), message.ReceiverAddress);
        parameters.Add(nameof(message.RequestDate), message.RequestDate, DbType.DateTime2);
        parameters.Add(nameof(message.RequestMessage), message.RequestMessage);
        parameters.Add(nameof(message.RequestOperation), message.RequestOperation);
        parameters.Add(nameof(message.ResponseDate), message.RequestDate, DbType.DateTime2);
        parameters.Add(nameof(message.ResponseStatus), message.ResponseStatus, DbType.Int32);
        parameters.Add(nameof(message.ResponseMessage), message.ResponseMessage);
        base.GetObjectCreateData(message, parameters);
    }
}