using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class PayrunJobEmployeeRepository() : ChildDomainRepository<PayrunJobEmployee>(DbSchema.Tables.PayrunJobEmployee,
    DbSchema.PayrunJobEmployeeColumn.PayrunJobId), IPayrunJobEmployeeRepository
{
    protected override void GetObjectData(PayrunJobEmployee user, DbParameterCollection parameters)
    {
        parameters.Add(nameof(user.EmployeeId), user.EmployeeId);
        base.GetObjectData(user, parameters);
    }
}