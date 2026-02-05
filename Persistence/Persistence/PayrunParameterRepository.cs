using System.Data;
using PayrollEngine.Domain.Model;
using PayrollEngine.Serialization;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class PayrunParameterRepository() : ChildDomainRepository<PayrunParameter>(DbSchema.Tables.PayrunParameter,
    DbSchema.PayrunParameterColumn.PayrunId), IPayrunParameterRepository
{
    protected override void GetObjectCreateData(PayrunParameter payrun, DbParameterCollection parameters)
    {
        parameters.Add(nameof(payrun.Name), payrun.Name);
        base.GetObjectCreateData(payrun, parameters);
    }

    protected override void GetObjectData(PayrunParameter parameter, DbParameterCollection parameters)
    {
        parameters.Add(nameof(parameter.NameLocalizations), JsonSerializer.SerializeNamedDictionary(parameter.NameLocalizations));
        parameters.Add(nameof(parameter.Description), parameter.Description);
        parameters.Add(nameof(parameter.DescriptionLocalizations), JsonSerializer.SerializeNamedDictionary(parameter.DescriptionLocalizations));
        parameters.Add(nameof(parameter.Mandatory), parameter.Mandatory, DbType.Boolean);
        parameters.Add(nameof(parameter.Value), parameter.Value);
        parameters.Add(nameof(parameter.ValueType), parameter.ValueType, DbType.Int32);
        parameters.Add(nameof(parameter.Attributes), JsonSerializer.SerializeNamedDictionary(parameter.Attributes));
        base.GetObjectData(parameter, parameters);
    }
}