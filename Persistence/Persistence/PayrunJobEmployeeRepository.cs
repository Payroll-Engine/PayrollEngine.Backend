using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class PayrunJobEmployeeRepository : ChildDomainRepository<PayrunJobEmployee>, IPayrunJobEmployeeRepository
{
    public PayrunJobEmployeeRepository() :
        base(DbSchema.Tables.PayrunJobEmployee, DbSchema.PayrunJobEmployeeColumn.PayrunJobId)
    {
    }

    protected override void GetObjectData(PayrunJobEmployee user, DbParameterCollection parameters)
    {
        parameters.Add(nameof(user.EmployeeId), user.EmployeeId);
        base.GetObjectData(user, parameters);
    }
}