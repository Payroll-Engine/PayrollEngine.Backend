using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class PayrunJobService : ChildApplicationService<IPayrunJobRepository, PayrunJob>, IPayrunJobService
{
    public PayrunJobServiceSettings Settings { get; }

    public PayrunJobService(
        PayrunJobServiceSettings settings) :
        base(settings.PayrunJobRepository)
    {
        Settings = settings;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PayrunJob>> QueryEmployeePayrunJobsAsync(IDbContext context, int tenantId, int employeeId, Query query = null) =>
        await Settings.PayrunJobRepository.QueryEmployeePayrunJobsAsync(context, tenantId, employeeId, query);

    /// <inheritdoc />
    public async Task<long> QueryEmployeePayrunJobsCountAsync(IDbContext context, int tenantId, int employeeId, Query query = null) =>
        await Settings.PayrunJobRepository.QueryEmployeePayrunJobsCountAsync(context, tenantId, employeeId, query);
}