using System.Data;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Persistence.DbSchema;

namespace PayrollEngine.Persistence;

public class PayrunJobEmployeeRepository() : ChildDomainRepository<PayrunJobEmployee>(Tables.PayrunJobEmployee,
    PayrunJobEmployeeColumn.PayrunJobId), IPayrunJobEmployeeRepository
{
    protected override void GetObjectData(PayrunJobEmployee user, DbParameterCollection parameters)
    {
        parameters.Add(nameof(user.EmployeeId), user.EmployeeId, DbType.Int32);
        base.GetObjectData(user, parameters);
    }
}