using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public abstract class CaseValueChangeRepository : ChildDomainRepository<CaseValueChange>, ICaseValueChangeRepository
{
    protected CaseValueChangeRepository(string tableName, string parentFieldName, IDbContext context) :
        base(tableName, parentFieldName, context)
    {
    }

    protected override void GetObjectCreateData(CaseValueChange valueChange, DbParameterCollection parameters)
    {
        parameters.Add(nameof(valueChange.CaseValueId), valueChange.CaseValueId);
        base.GetObjectCreateData(valueChange, parameters);
    }
}