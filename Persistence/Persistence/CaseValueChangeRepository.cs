using System.Data;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public abstract class CaseValueChangeRepository
    (string tableName, string parentFieldName) : ChildDomainRepository<CaseValueChange>(tableName, parentFieldName),
        ICaseValueChangeRepository
{
    protected override void GetObjectCreateData(CaseValueChange valueChange, DbParameterCollection parameters)
    {
        parameters.Add(nameof(valueChange.CaseValueId), valueChange.CaseValueId, DbType.Int32);
        base.GetObjectCreateData(valueChange, parameters);
    }
}